import { Component, OnInit, Injector, ViewChild } from '@angular/core';
import { PagedListingComponentBase, PagedRequestDto } from '@shared/paged-listing-component-base';
import { FormControl } from '@angular/forms';
import { finalize } from 'rxjs/operators';
import { CheckBoardService } from '@app/service/api/check-board.service';
import { DatePipe } from '@angular/common';
import { MatDialog, MatMenuTrigger } from '@angular/material';
import { CreateCheckBoardComponent } from './create-check-board/create-check-board.component';
import * as _ from 'lodash';

@Component({
  selector: 'app-check-board',
  templateUrl: './check-board.component.html',
  styleUrls: ['./check-board.component.css']
})
export class CheckBoardComponent extends PagedListingComponentBase<CheckBoardDTO> implements OnInit {

  isLoadingFileUpload: boolean;
  isFileSupported: boolean;
  fileSCROM: File;
  startDate: any;
  endDate: any;
  loading = false;
  inputStartDate: FormControl = new FormControl('');
  inputEndDate: FormControl = new FormControl('');
  inputSearch: FormControl = new FormControl('');
  defaultYear = 0;
  defaultMonth = 0;
  searchYear = 0;
  searchMonth = 0;
  monthListBase = [monthList.January, monthList.February, monthList.March, monthList.April, monthList.May,
  monthList.June, monthList.July, monthList.August, monthList.September, monthList.October,
  monthList.Novenber, monthList.December
  ]
  yearList = [];
  monthList = [];
  listCheckBoard: CheckBoardDTO[] = [];
  datePipe: DatePipe;
  contextMenuPosition = { x: '0px', y: '0px' };

  constructor(
    injector: Injector,
    private checkBoardService: CheckBoardService,
    private dialog: MatDialog
  ) {
    super(injector);
    const d = new Date();
    this.defaultYear = d.getFullYear();
    this.defaultMonth = d.getMonth() + 1;
    for (let i = this.defaultYear - 20; i <= this.defaultYear; i++) {
      this.yearList.push(i);
    }
    this.monthList = this.monthListBase.slice(monthList.January - 1 , d.getMonth() + 1);
    this.searchMonth = d.getMonth() + 1;
    this.searchYear = this.defaultYear;
  }

  onContextMenu(event: MouseEvent, temp) {
    event.preventDefault();
    this.contextMenuPosition.x = event.clientX + 'px';
    this.contextMenuPosition.y = event.clientY + 'px';
    temp.openMenu();
  }

  protected list(
    request: PagedRequestDto,
    pageNumber: number,
    finishedCallback: Function
  ): void {
    if (this.inputSearch.value) {
      request.searchText = this.inputSearch.value.trim().toLowerCase();
    }
    this.checkBoardService
      .getAll(request, this.searchMonth, this.searchYear)
      .pipe(finalize(() => {
        finishedCallback();
      }))
      .subscribe((result: any) => {
        this.listCheckBoard = result.result.items;
        this.listCheckBoard.forEach(data => {
          var d = new Date(data.dateAt);
          data.dateAt = d.getMonth() + 1 + "/" + d.getDate() + "/" + d.getFullYear();
        });
        this.showPaging(result.result, pageNumber);
      });
  }

  protected delete(entity: CheckBoardDTO): void {
    abp.message.confirm(this.l("Delete This Check Board?"),
      (result: boolean) => {
        if(result) {
          this.checkBoardService.deleteCheckBoard(entity).subscribe(data => {
            if(data) {
              this.notify.success(this.l("Delete Check Board Successfully!"));
              this.refresh();
            }
          })
        }
      }
    )
  }

  updateMonths(): void {
    var d = new Date();
    if(this.defaultYear < d.getFullYear()) {
      this.monthList = this.monthListBase;
    }
    else {
      this.monthList = this.monthListBase.slice(monthList.January - 1 , d.getMonth() + 1);
    }
    this.defaultMonth = 0;
  }

  upLoadCheckBoards(file: File) {
    if (!file.name.toLowerCase().endsWith('xlsx')) {
      this.isFileSupported = false;
      return;
    }
    this.isLoadingFileUpload = true;
    this.fileSCROM = file;

    this.checkBoardService.uploadFile(file).subscribe(data => {
      this.isLoadingFileUpload = false;
      let result = data.body.result;
        if(result.failedList.length > 0){
          let info = '<b>Success: ' + result.successList.length + 'Failed: ' + result.failedList.length + '</b><ul>';
          result.failedList.forEach(elem => {
            info += `<li>${elem}</li>`;
          });
          info += '<ul>'
          abp.message.info(info, 'Upload result', true);
          this.refresh();
        }
        else {
          abp.message.success('Success', 'Upload result');
          this.refresh();
        }
    }, () => {
      this.isLoadingFileUpload = false;
      abp.notify.error(this.l("The file is not supported!"));
    });
    
  }

  search(): void {
    if (this.defaultMonth == 0) {
      this.notify.error(this.l('Please choose a month to search'));
      return;
    }
    this.searchMonth = this.defaultMonth;
    this.searchYear = this.defaultYear;
    this.pageNumber = 1;
    this.pageSize = 10;
    this.totalPages = 1;
    this.refresh();
  }

  confirmCheckBoard(): void {
    abp.message.confirm("Confirm checkboard of month " + this.defaultMonth,
      (result: boolean) => {
        if (result) {
          let param = { month: this.defaultMonth, year: this.defaultYear };
          this.loading = true;
          this.checkBoardService.confirmCheckBoard(param).subscribe(res => {
            this.loading = false;
            if (res.result.success) {
              this.notify.success(this.l('Confirm successfully!'));
            } else {
              var str = _.startCase(res.result.errorMsg);
              var strList = str.split(" ");
              let error = "<h3>You have to delete some duplicate checkboards!</h3><br>"
              error += "<ul>";
              for(let i = 0; i< strList.length; i+=9) {
                var day = strList[i+1] + ' - ' + strList[i+2] + ' - ' + strList[i+3];
                error += "<li> UserCode: " + strList[i] + " - Day: " + day + " - Number of Check Board: "+ strList[i+8] + "</li>";
              }
              error += "</ul>";
              abp.message.info(error, "Confirm Error", true);
            }
            this.refresh();
          });
        }
      }
    )
  }

  createNewOrEditCheckBoard(cb?): void {
    var checkBoard = cb? cb : null;
    let diaLogRef = this.dialog.open(CreateCheckBoardComponent, {
      data: {item: checkBoard},
      width: "500px"
    });
    diaLogRef.afterClosed().subscribe(() => this.refresh());
  }

}

export class CheckBoardDTO {
  cbUserName: string;
  userCode: string;
  status: number;
  checkInAt: string;
  checkOutAt: string;
  checkInLate: number;
  checkOutEarly: number;
  workingDay: number;
  workingHour: number;
  dateAt: string;
  emailAddress: string;
  fullName: string;
  note: string;
  id: number;
}

export enum monthList {
  January = 1,
  February = 2,
  March = 3,
  April = 4,
  May = 5,
  June = 6,
  July = 7,
  August = 8,
  September = 9,
  October = 10,
  Novenber = 11,
  December = 12
}
