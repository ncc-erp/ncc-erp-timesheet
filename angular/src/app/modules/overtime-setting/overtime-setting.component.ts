import { CreateEditOvertimeSettingComponent } from './create-edit-overtime-setting/create-edit-overtime-setting.component';
import { MatDialog } from '@angular/material';
import { appModuleAnimation } from '@shared/animations/routerTransition';
import { OvertimeSettingService } from '../../service/api/overtime-setting.service';
import { Component, OnInit, Injector } from '@angular/core';
import { PagedListingComponentBase, PagedRequestDto } from '@shared/paged-listing-component-base';
import { finalize } from 'rxjs/operators';
import { PERMISSIONS_CONSTANT } from '@app/constant/permission.constant';
import * as moment from 'moment';
import { ProjectManagerService } from '@app/service/api/project-manager.service';
import { FormControl } from '@angular/forms';


@Component({
  selector: 'app-overtimeSetting',
  templateUrl: './overtime-setting.component.html',
  styleUrls: ['./overtime-setting.component.css'],
  animations: [appModuleAnimation()]
})

export class OvertimeSettingComponent extends PagedListingComponentBase<OvertimeSettingDto> implements OnInit {
  VIEW = PERMISSIONS_CONSTANT.ViewOverTimeSetting;
  ADD = PERMISSIONS_CONSTANT.AddNewOverTimeSetting;
  EDIT = PERMISSIONS_CONSTANT.EditOverTimeSetting;
  DELETE = PERMISSIONS_CONSTANT.DeleteOverTimeSetting;
  isActive: boolean | null;
  dateAt : string;
  projectFilter = []
  projectId = -1;
  projectSearch: FormControl = new FormControl("")
  projects = []
  overtimeSettings = [] as OvertimeSettingDto[];

  constructor(
    private overtimeSettingService: OvertimeSettingService,
    private projectManageService: ProjectManagerService,
    private dialog: MatDialog,
    injector: Injector
  ) {
    super(injector);
    this.projectSearch.valueChanges.subscribe(() => {
      this.filterProject();
    });
  }

  ngOnInit() {
    this.getProjects();
    this.dateAt = "";
    this.refresh();
  }

  getProjects() {
    this.projectManageService.getProjectFilter().subscribe(data => {
      this.projectFilter = data.result
      this.projects = this.projectFilter
      this.projects.unshift({
        id: -1,
        name: "All"
      })
    })
  }
  filterProject(): void {
    if (this.projectSearch.value) {
      const temp: string = this.projectSearch.value.toLowerCase().trim();
      this.projects = this.projectFilter.filter(data => data.name.toLowerCase().includes(temp));
    } else {
      this.projects = this.projectFilter.slice();
    }
  }

  changeDate() {
    this.dateAt = moment(this.dateAt).format('YYYY-MM-DD');
    this.refresh();
  }

  formatDate(date: string) {
    return moment(date).format("DD-MM-YYYY");
  }
  
  protected list(request: PagedRequestDto, pageNumber: number, finishedCallback: Function): void {
    if (this.searchText) {
      request.searchText = this.searchText;
    }
    
    this.overtimeSettingService
      .getAll(request, this.dateAt, this.projectId)
      .pipe(finalize(() => {
        finishedCallback();
      }))
      .subscribe((result: any) => {
        this.overtimeSettings = result.result.items;
        this.showPaging(result.result, pageNumber);
      });

  }

  protected delete(item: OvertimeSettingDto): void {
    abp.message.confirm(
      "Delete Overtime setting project '" + item.projectName + "'?",
      (result: boolean) => {
        if (result) {
          this.overtimeSettingService.delete(item.id).subscribe(() => {
            abp.notify.info('Deleted Overtime setting project: ' + item.projectName);
            this.refresh();
          });
        }
      }
    );
  }

  create(): void {
    let overtimeSetting = {} as OvertimeSettingDto;
    this.showDialog(overtimeSetting);
  }

  edit(overtimeSetting: OvertimeSettingDto): void {
   
    this.showDialog(overtimeSetting);
  }

  showDialog(overtimeSetting: OvertimeSettingDto): void {
    let item = { 
      id: overtimeSetting.id,
      note: overtimeSetting.note,
      dateAt: overtimeSetting.dateAt,
      projectId: overtimeSetting.projectId,
      coefficient: overtimeSetting.coefficient
    } as OvertimeSettingDto;
    const dialogRef = this.dialog.open(CreateEditOvertimeSettingComponent, {
      data: item,
      disableClose : true
    });

    dialogRef.afterClosed().subscribe((res) => {
      if(res) {
        if (overtimeSetting.id == null) {
          this.getDataPage(1);
        }
        else {
          this.getDataPage(this.pageNumber);
        }
      }
    });
  }
}

export class OvertimeSettingDto {
  id: number;
  note: string;
  projectName: string;
  dateAt: string;
  projectId: number;
  coefficient: number;
}
