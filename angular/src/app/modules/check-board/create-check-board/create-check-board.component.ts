import { Component, OnInit, Inject, Injector } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material';
import { CheckBoardDTO } from '../check-board.component';
import { FormControl, Validators } from '@angular/forms';
import { CheckBoardService } from '@app/service/api/check-board.service';
import { AppComponentBase } from '@shared/app-component-base';
import { UserService } from '@app/service/api/user.service';
import * as _ from 'lodash';
import * as moment from 'moment';

@Component({
  selector: 'app-create-check-board',
  templateUrl: './create-check-board.component.html',
  styleUrls: ['./create-check-board.component.css']
})
export class CreateCheckBoardComponent extends AppComponentBase implements OnInit {

  title;
  checkBoard = {} as CheckBoardDTO;
  checkBoardTransfer: checkBoardCreateOrUpdateDTO = {} as checkBoardCreateOrUpdateDTO;
  listUserBase: userDTO[] = [];
  listUserFiltered: userDTO[];
  dateControl: FormControl = new FormControl();
  isEdit: boolean;
  selectedId = 0;
  userSearch = new FormControl();
  userControl = new FormControl("", [Validators.required]);
  constructor(
    injector: Injector,
    private userService: UserService,
    private checkBoardService: CheckBoardService,
    @Inject(MAT_DIALOG_DATA) public data: any,
    private diaLogRef: MatDialogRef<CreateCheckBoardComponent>
  ) {
    super(injector);
    this.userSearch.valueChanges.subscribe(() => {
      this.search();
    });
  }

  ngOnInit() {
    this.userService.getAllNotPagging().subscribe(result => {
      this.listUserBase = result.result;
      this.init();
    });
  }

  init(): void {
    if (this.data.item) {
      this.checkBoard = this.data.item;
    }
    this.isEdit = this.checkBoard.cbUserName != null;
    this.title = this.isEdit ? 'Edit Checkboard for "' + this.checkBoard.fullName + '" in ' + this.checkBoard.dateAt : 'Create New Checkboard';
    this.userControl.setValue(this.checkBoard.userCode);
    this.checkBoardTransfer.note = this.checkBoard.note;
    this.checkBoardTransfer.dateAt = this.checkBoard.dateAt;
    this.checkBoardTransfer.workingHour = this.checkBoard.workingHour;
    this.listUserFiltered = this.listUserBase.slice();
    if (this.checkBoard.dateAt) {
      this.dateControl = new FormControl(new Date(this.checkBoard.dateAt), [Validators.required]);
    }
  }

  search(): void {
    var temp = this.userSearch.value.trim().toLowerCase();
    this.listUserFiltered = this.listUserBase.filter(data =>
      data.name.toLowerCase().includes(temp)
    );
  }

  onNoClick(): void {
    this.diaLogRef.close();
  }

  checkUserCode(): void {
    if(this.userControl.value == null) {
      abp.message.error(this.l("You need to update Code for this user first!"));
    }
  }

  save(): void {
    this.checkBoardTransfer.userId = this.getUserId();
    var date = new Date(this.dateControl.value);
    if (isNaN(date.getTime())) {
      abp.message.error('Please enter a valid date!');
      return;
    }
    if (isNaN(this.checkBoardTransfer.workingHour)) {
      abp.message.error('Working hour must be a number!');
      return;
    }
    this.checkBoardTransfer.workingHour = +this.checkBoardTransfer.workingHour;
    if(this.checkBoardTransfer.workingHour < 0 || this.checkBoardTransfer.workingHour > 8) {
      abp.message.error("Working hour must bigger than 0 and smaller than 8!");
      return;
    }
    this.checkBoardTransfer.dateAt = new Date(moment(date).add(0, 'day').format("YYYY-MM-DD")).toISOString();
    if (!this.isEdit) {
      this.checkBoardService.createCheckBoard(this.checkBoardTransfer).subscribe(result => {
        if (result) {
          this.onNoClick();
          this.notify.success(this.l('Create Check Board Successfully!'));
        }
      });
    } else {
      this.checkBoard.workingHour = +this.checkBoardTransfer.workingHour;
      this.checkBoard.note = this.checkBoardTransfer.note;
      this.checkBoard.dateAt = new Date(this.checkBoard.dateAt).toISOString();
      this.checkBoardService.updateCheckBoard(this.checkBoard).subscribe(result => {
        if(result) {
          this.onNoClick();
          this.notify.success(this.l("Edit Check Board Successfully!"));
        }
      });
    }
  }

  getUserId(): number {
    return _(this.listUserBase).find(data => data.userCode == this.userControl.value).id;
  }
}

export class checkBoardCreateOrUpdateDTO {
  userId: number;
  workingHour: number;
  dateAt: string;
  note: string;
}

export class userDTO {
  name: string;
  isActive: boolean;
  type: number;
  jobTitle: any;
  level: any;
  userCode: string;
  id: number;
}
