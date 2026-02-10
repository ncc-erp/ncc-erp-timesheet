import { Component, Inject, Injector, OnInit } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material';
import { AppComponentBase } from '@shared/app-component-base';
import { UserService } from '@app/service/api/user.service';
import { FormControl } from '@angular/forms';
import { userDTO } from '@app/modules/user/user.component';
import { RemoteBlacklistService } from '@app/service/api/remote-blacklist.service';
import { AddNewUserToRemoteBlacklistDto, GetRemoteBlacklistDto } from '@app/service/api/model/remote-blacklist.dto';
import { finalize } from 'rxjs/operators';

@Component({
  selector: 'app-add-user-to-remote-blacklist-dialog',
  templateUrl: './add-user-to-remote-blacklist-dialog.component.html',
  styleUrls: ['./add-user-to-remote-blacklist-dialog.component.css']
})
export class AddUserToRemoteBlacklistDialogComponent extends AppComponentBase implements OnInit {

  userId: number;
  penaltyDays: number;
  saving: boolean = false;
  userSearch: FormControl = new FormControl();
  listUserBase: userDTO[] = [];
  listUserFiltered: userDTO[] = [];
  maxAllowedRemoteDays: number = 0;

  constructor(
    injector: Injector,
    public dialogRef: MatDialogRef<AddUserToRemoteBlacklistDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: any,
    private userService: UserService,
    private remoteBlacklistService: RemoteBlacklistService
  ) {
    super(injector);
  }

  ngOnInit(): void {
    if (this.data && this.data.maxAllowedRemoteDays) {
      this.maxAllowedRemoteDays = this.data.maxAllowedRemoteDays;
    } else {
      this.getMaxRemoteDays(); 
    }
    this.getAllUsers();
    this.userSearch.valueChanges.subscribe(() => {
      this.filterUsers();
    });
  }

  getAllUsers() {
    this.userService.getAllNotPagging().subscribe(rs => {
      this.listUserBase = rs.result;
      this.listUserFiltered = this.listUserBase;
    });
  }

  getMaxRemoteDays() {
    this.remoteBlacklistService.getMaxRemoteDays().subscribe(res => {
      this.maxAllowedRemoteDays = res.result;
    });
  }

  filterUsers(): void {
    if (!this.listUserBase) {
      return;
    }
    let search = this.userSearch.value;
    if (!search) {
      this.listUserFiltered = this.listUserBase;
      return;
    } else {
      search = search.toLowerCase();
    }

    this.listUserFiltered = this.listUserBase.filter(user =>
      (user.userName && user.userName.toLowerCase().indexOf(search) > -1) ||
      (user.emailAddress && user.emailAddress.toLowerCase().indexOf(search) > -1) || 
      (user.fullName && user.fullName.toLowerCase().indexOf(search) > -1)
    );
  }

  preventNegative(event: KeyboardEvent): void {
    if (['-', '+', 'e', '.'].indexOf(event.key) > -1) {
      event.preventDefault();
    }
  }

  save(): void {
    if (!this.userId) {
      abp.message.error("Username is required!");
      return;
    }

    if (!this.penaltyDays || this.penaltyDays > this.maxAllowedRemoteDays) {
      abp.message.error(`Penalty days must be a valid number between 1 and ${this.maxAllowedRemoteDays}!`);
      return;
    }

    this.saving = true;

    const input = new AddNewUserToRemoteBlacklistDto(this.userId, this.penaltyDays);

    this.remoteBlacklistService.addNewUser(input)
      .pipe(finalize(() => { this.saving = false; }))
      .subscribe(
        (result: any) => {
          if (result) {
            this.notify.success(this.l('Saved Successfully'));
            this.dialogRef.close(result);
          }
        },
        () => {
          this.saving = false;
        },
      );
  }

  close(): void {
    this.dialogRef.close();
  }
}