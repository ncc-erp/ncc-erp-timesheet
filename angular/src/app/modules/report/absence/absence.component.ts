import {Component, Injector, OnInit} from '@angular/core';
import {FormControl} from '@angular/forms';
import {BranchDto} from '@shared/service-proxies/service-proxies';
import {PositionDto} from '@app/service/api/model/position-dto';
import {BranchService} from '@app/service/api/branch.service';
import {PositionService} from '@app/service/api/position.service';
import {AbsenceDayService} from '@app/service/api/absence-day.service';
import {AppSessionService} from '@shared/session/app-session.service';
import {AbsenceReportRequest} from '@app/service/api/model/absence-day-dto';
import * as moment from 'moment';
import {MatDatepicker, PageEvent} from '@node_modules/@angular/material';
import {PagedRequestDto} from '@shared/paged-listing-component-base';
import {PagedListingComponentBase} from '@shared/paged-listing-component-base';
import { PERMISSIONS_CONSTANT } from '@app/constant/permission.constant';
import { log } from 'console';
import { Moment } from 'moment';
import { MAT_DATE_FORMATS } from '@angular/material/core';

@Component({
  selector: 'app-absence',
  templateUrl: './absence.component.html',
  styleUrls: ['./absence.component.css']
})
export class AbsenceComponent extends PagedListingComponentBase<any> implements OnInit {

  AbsenceReport_View = PERMISSIONS_CONSTANT.ViewAbsenceDayByBranch;

  searchText: any;

  branchId: any;
  listBranch: BranchDto[] = [];
  branchSearch: FormControl = new FormControl('');
  listBranchFilter: BranchDto[];

  positionId: any;
  listPosition: PositionDto[] = [];
  positionSearch: FormControl = new FormControl('');
  listPositionFilter: PositionDto[];

  requestTypes = [
    {value: 2, label: 'Remote'},
    {value: 0, label: 'Off'}
  ];

  absenceReportRequestDto: AbsenceReportRequest;
  absenceReportResult: any[] = [];
  displayedColumns: any[] = ['index', 'user', 'position', 'totalTime'];
  displayDay: any;
  selectedTimeRange: string;
  isLoading: boolean = false;
  isDisabled: boolean = false;

  monthYearControl = new FormControl();
  selectedMonthYear: Moment ;



  constructor(
    injector: Injector,
    private branchService: BranchService,
    private positionService: PositionService,
    appSession: AppSessionService,
    private absenceDayService: AbsenceDayService
  ) {
    super(injector);
    this.branchService = injector.get(BranchService);
    this.positionService = injector.get(PositionService);
    this.appSession = injector.get(AppSessionService);
    this.absenceDayService = injector.get(AbsenceDayService);
    this.absenceReportRequestDto = new AbsenceReportRequest();

    this.branchSearch.valueChanges.subscribe(() => {
      this.filterBranches();
    });

    this.positionSearch.valueChanges.subscribe(() => {
      this.filterPosition();
    });

  }

  ngOnInit(): void {
    this.getListBranch();
    this.getListPosition();
    this.selectedTimeRange = 'DAY';

    this.loadInitFormData();

  }

  loadInitFormData() {
    this.absenceReportRequestDto.branchId = this.appSession.user.branchId;
    this.absenceReportRequestDto.positionId = 0;
    this.absenceReportRequestDto.requestType = 0;
    this.absenceReportRequestDto.email = '';
    this.absenceReportRequestDto.skipCount = 0;
    this.absenceReportRequestDto.maxResultCount = 10;
    this.pageSize = 10;
    this.totalItems = 0;
    this.displayDay = moment().format('YYYY-MM-DD');
    this.viewBy(this.selectedTimeRange);
    this.updateTimeRange();

  }

  getListBranch() {
    this.branchService.getAllBranchFilter(true).subscribe(res => {
      this.listBranch = res.result;
      this.listBranchFilter = this.listBranch;
    });
  }

  filterBranches(): void {
    if (this.branchSearch.value) {
      const temp: string = this.branchSearch.value.toLowerCase().trim();
      this.listBranch = this.listBranchFilter.filter(data => data.name.toLowerCase().includes(temp));
    } else {
      this.listBranch = this.listBranchFilter.slice();
    }
  }

  getListPosition() {
    this.positionService.getAllFilter().subscribe(res => {
      this.listPosition = res.result;
      this.listPositionFilter = this.listPosition;
    });
  }

  filterPosition(): void {
    if (this.positionSearch.value) {
      const temp: string = this.positionSearch.value.toLowerCase().trim();
      this.listPosition = this.listPositionFilter.filter(data => data.name.toLowerCase().includes(temp));
    } else {
      this.listPosition = this.listPositionFilter.slice();
    }
  }

  protected list(request: PagedRequestDto, pageNumber: number, finishedCallback: Function): void {
    this.isLoading = true;
    
    this.absenceReportRequestDto.skipCount = request.skipCount;
    this.absenceReportRequestDto.maxResultCount = request.maxResultCount;
    
    this.absenceDayService.getAbsenceReport(this.absenceReportRequestDto)
      .subscribe(data => {
        if (data.result && data.result.items) {
          this.absenceReportResult = data.result.items;
          this.totalItems = data.result.totalCount;
          this.showPaging(data.result, pageNumber);
        } else {
          this.absenceReportResult = [];
          this.totalItems = 0;
        }
        this.isLoading = false;
        finishedCallback();
      });
  }
  
  onFilter() {
    this.getDataPage(1); 
  }

  clearFilters() {
    this.loadInitFormData();
  }

  back() {
    this.displayDay = moment(this.displayDay).subtract(1, 'days').format('YYYY-MM-DD');
    this.updateTimeRange();
  }

  next() {
    this.displayDay = moment(this.displayDay).add(1, 'days').format('YYYY-MM-DD');
    this.updateTimeRange();
  }

  updateTimeRange() {
    switch (this.selectedTimeRange) {
      case 'DAY': {
        const startDate = new Date(this.displayDay);
        startDate.setHours(0, 0, 0, 0);
        this.absenceReportRequestDto.startDate = startDate.toISOString();

        const endDate = new Date(this.displayDay);
        endDate.setHours(23, 59, 59, 999);
        this.absenceReportRequestDto.endDate = endDate.toISOString();
        break;
      }
      case 'WEEK': {
        const d = new Date(this.displayDay);

        // Get the day of the week (0 = Sunday, 1 = Monday, ..., 6 = Saturday)
        const day = d.getDay();

        // Calculate difference to Monday
        const diffToMonday = day === 0 ? -6 : 1 - day;

        const monday = new Date(d);
        monday.setDate(d.getDate() + diffToMonday);
        monday.setHours(0, 0, 0, 0); // Start of Monday
        this.absenceReportRequestDto.startDate = monday.toISOString();

        const sunday = new Date(monday);
        sunday.setDate(monday.getDate() + 6);
        sunday.setHours(23, 59, 59, 999);
        this.absenceReportRequestDto.endDate = sunday.toISOString();
        break;
      }
      case 'MONTH': {
        const startDate = new Date(new Date(this.displayDay).getFullYear(), new Date(this.displayDay).getMonth(), 1);
        startDate.setHours(0, 0, 0, 0);
        this.absenceReportRequestDto.startDate = startDate.toISOString();

        const endDate = new Date(new Date(this.displayDay).getFullYear(), new Date(this.displayDay).getMonth() + 1, 0);
        endDate.setHours(23, 59, 59, 999);
        this.absenceReportRequestDto.endDate = endDate.toISOString();
        break;
      }
    }
  }




  viewBy(range: string) {
    this.selectedTimeRange = range;
    this.updateTimeRange();
  }

  onMatPaginatorChange(event: PageEvent): void {
    this.pageSize = event.pageSize;
    const pageIndex = event.pageIndex;
    this.getDataPage(pageIndex + 1); 
  }

  protected delete(entity: any): void {
  }
}

