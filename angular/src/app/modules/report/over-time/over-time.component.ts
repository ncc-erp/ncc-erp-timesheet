import { FormControl } from '@angular/forms';
import { ProjectManagerService } from '@app/service/api/project-manager.service';
import { OverTimeService } from '../../../service/api/over-time.service';
import { PagedListingComponentBase, PagedRequestDto } from 'shared/paged-listing-component-base';
import { Component, OnInit, Injector } from '@angular/core';
import { finalize } from 'rxjs/operators';
import * as moment from 'moment';
import { APP_CONSTANT } from '@app/constant/api.constants';

@Component({
  selector: 'app-over-time',
  templateUrl: './over-time.component.html',
  styleUrls: ['./over-time.component.css']
})
export class OverTimeComponent extends PagedListingComponentBase<OverTimeItem> implements OnInit {

  month;
  year;
  listMonth = [0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11];
  listYear = APP_CONSTANT.ListYear;
  listOverTime: OverTimeItem[] = [];
  searchText = '';
  projectFilter = []
  projectId = -1;
  projectSearch: FormControl = new FormControl("")
  projects = []
  isLoading: boolean;
  constructor(
    injector: Injector,
    private overTimeService: OverTimeService,
    private projectManageService: ProjectManagerService,
  ) {
    super(injector);
    var d = new Date();
    d.setMonth(d.getMonth());
    this.month = d.getMonth();
    this.year = d.getFullYear();
    this.projectSearch.valueChanges.subscribe(() => {
      this.filterProject();
    });

  }

  ngOnInit() {
    this.getProjects();
    this.refresh();
  }

  protected list(request: PagedRequestDto, pageNumber: number, finishedCallback: Function): void {
    if (this.searchText) {
      request.searchText = this.searchText;
    }

    this.overTimeService
      .getAll(request, this.month + 1, this.year, this.projectId)
      .pipe(finalize(() => {
        finishedCallback();
      }))
      .subscribe((result: any) => {
        this.listOverTime = result.result.items;
        this.listOverTime.forEach(data => {
          data.totalHour = 0;
          data.totalWorkingHour = 0;
          data.listOverTimeHour.forEach(d => {
            data.totalHour += d.otHour;
            data.totalWorkingHour += d.workingHour;
            d.date = moment(d.date).format("DD/MM/YYYY");
          });
        });
        this.showPaging(result.result, pageNumber);
      });

  }
  protected delete(entity: any): void {

  }

  formatHour(time) {
    const hours = Math.floor(time);
    const rhours = ('0' + hours).slice(-2);
    const minutes = (time - hours) * 60;
    const rminutes = ('0' + minutes).slice(-2);
    return rhours + ':' + rminutes;
  }

  getProjects() {
    this.projectManageService.getProjectFilter().subscribe(data => {
      this.projectFilter = data.result
      this.projects = this.projectFilter
      this.projects.unshift({
        id: -1,
        code: "All"
      })
    })

  }

  filterProject(): void {
    if (this.projectSearch.value) {
      const temp: string = this.projectSearch.value.toLowerCase().trim();
      this.projects = this.projectFilter.filter(data => data.code.toLowerCase().includes(temp));
    } else {
      this.projects = this.projectFilter.slice();
    }
  }

}

export class OverTimeHour {
  date: string;
  dayName: string;
  otHour: number;
  coefficient: number;
  workingHour: number
}

export class OverTimeItem {
  userId: number;
  fullName: string;
  listOverTimeHour: OverTimeHour[];
  totalWorkingHour: number;
  totalHour: number;
}
