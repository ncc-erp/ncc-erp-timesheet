import { Component, OnInit, Injector } from '@angular/core';
import { PagedListingComponentBase, PagedRequestDto } from '@shared/paged-listing-component-base';
import { DayOffService } from '@app/service/api/day-off.service';
import { finalize } from 'rxjs/operators';
import { DatePipe } from '@angular/common';
import { MatDialog } from '@angular/material';
import { CreateEditDayOffComponent } from './create-edit-day-off/create-edit-day-off.component';
import { PERMISSIONS_CONSTANT } from '@app/constant/permission.constant';

@Component({
  selector: 'app-day-off',
  templateUrl: './day-off.component.html',
  styleUrls: ['./day-off.component.css'],
  providers: [DatePipe]
})
export class DayOffComponent extends PagedListingComponentBase<dayOffDTO> implements OnInit {
  
  keyword;
  contextMenuPosition = { x: '0px', y: '0px' };
  listDayOff: dayOffDTO[];
  listYear = [];
  listMonth = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12];
  year;
  month;
  constructor(
    injector: Injector,
    private dayOffService: DayOffService,
    private datePipe: DatePipe,
    private diaLog: MatDialog
  ) {
    super(injector);
    var d = new Date().getFullYear();
    for(let  i = d - 5; i <= d+2; i++) {
      this.listYear.push(i);
    }
    this.year = d;
  }

  protected list(request: PagedRequestDto, pageNumber: number, finishedCallback: Function): void {
    if(this.keyword && this.keyword.trim()) {
      request.searchText = this.keyword;
    }
    this.dayOffService
      .getAllDayOff(request, this.month, this.year)
      .pipe(
        finalize(() => {
          finishedCallback();
        })
      )
      .subscribe(data => {
        this.listDayOff = data.result.items;
        this.listDayOff.forEach(day => {
          day.dayOff = this.datePipe.transform(day.dayOff, "MM-dd-yyyy");
        });
        this.showPaging(data.result, pageNumber);
      });
  }
  protected delete(id): void {
    abp.message.confirm(this.l("Delete This Day Off?"), (result: boolean) => {
      if(result) {
        this.dayOffService.deleteDayOff(id).subscribe(res => {
          if (res) {
            this.notify.success(this.l("Delete Successfully!"));
          }
          this.refresh();
        });
      }
    });
  }

  onContextMenu(event: MouseEvent, menu): void {
    event.preventDefault();
    this.contextMenuPosition.x = event.clientX + 'px';
    this.contextMenuPosition.y = event.clientY + 'px';
    menu.openMenu();
  }

  createOrEdit(day?: dayOffDTO): void {
    var temp = day == null ? {} as dayOffDTO : day;
    var diaLogRef = this.diaLog.open(CreateEditDayOffComponent, {
      data: {item: temp},
      width: "600px"
    });
    diaLogRef.afterClosed().subscribe(() => {
      this.refresh();
    });
  }

  searchOrFilter(): void {
    this.refresh();
  }

}

export class dayOffDTO {
  dayOff: string;
  name: string;
  id: number;
  coefficient: any;
}
