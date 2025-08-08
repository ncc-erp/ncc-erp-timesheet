import { AppComponentBase } from '@shared/app-component-base';
import { MatDialogRef, MAT_DIALOG_DATA, MatAutocompleteSelectedEvent } from '@angular/material';
import { Component, Injector, Inject, OnInit, AfterViewInit, ViewChild } from '@angular/core';
import { NgForm } from '@angular/forms';
import { finalize } from 'rxjs/operators';
import { UserPunishmentService, CreateUserPunishmentDto, UpdateUserPunishmentDto, UserPunishmentDto } from '@app/service/api/user-punishment.service';
import { APP_CONSTANT } from '@app/constant/api.constants';
import { UserService } from '@app/service/api/user.service';
import * as moment from 'moment';

export interface IPunishmentType {
  value: number;
  name: string;
}

@Component({
  selector: 'app-create-edit-user-punishment',
  templateUrl: './create-edit-user-punishment.component.html',
  styleUrls: ['./create-edit-user-punishment.component.css']
})
export class CreateEditUserPunishmentComponent extends AppComponentBase implements OnInit, AfterViewInit {
  userPunishment: CreateUserPunishmentDto | UpdateUserPunishmentDto;
  isEditMode = false;
  title: string;
  active = true;
  isSaving = false;
  
  punishmentTypes: any[] = [];
  isLoadingTypes = true;
  users: any[] = [];
  filteredUsers: any[] = [];
  isLoadingUsers = true;
  selectedDate: Date = new Date();
  userSearchText = '';

  @ViewChild('createForm') createForm: NgForm;

  constructor(
    @Inject(MAT_DIALOG_DATA) public data: any,
    injector: Injector,
    private _userPunishmentService: UserPunishmentService,
    private _userService: UserService,
    private _dialogRef: MatDialogRef<CreateEditUserPunishmentComponent>
  ) {
    super(injector);
  }

  ngOnInit() {
    this.initializeUserPunishment();
    this.loadPunishmentTypes();
    this.loadUsers();
  }

  initializeUserPunishment() {
    if (this.data && this.data.id) {
      this.isEditMode = true;
      this.userPunishment = { 
        id: this.data.id,
        dateAt: this.data.dateAt,
        userId: this.data.userId,
        type: this.data.type,
        money: this.data.totalMoney,
        noteReply: this.data.noteReply || ''
      } as UpdateUserPunishmentDto;
      this.selectedDate = new Date(this.userPunishment.dateAt);
      this.title = 'Edit User Punishment';
    } else {
      this.isEditMode = false;
      this.userPunishment = {
        dateAt: this.data && this.data.date ? moment(this.data.date).format('YYYY-MM-DD') : moment().format('YYYY-MM-DD'),
        userId: this.data && this.data.userId ? this.data.userId : null,
        type: null,
        money: 0,
        noteReply: ''
      } as CreateUserPunishmentDto;
      this.selectedDate = this.data && this.data.date ? new Date(this.data.date) : new Date();
      this.title = 'New User Punishment';
    }
  }

  loadPunishmentTypes() {
    this.isLoadingTypes = true;
    this.punishmentTypes = APP_CONSTANT.PunishRules.filter(type => 
      type.value === 15 || type.value === 16
    );
    this.isLoadingTypes = false;
  }
  
  loadUsers() {
    this.isLoadingUsers = true;
    this._userService.getAllNotPagging().subscribe(
      (result) => {
        if (result && result.success) {
          this.users = result.result;
          this.filteredUsers = [...this.users];
        }
        this.isLoadingUsers = false;
      },
      () => {
        this.isLoadingUsers = false;
      }
    );
  }
  
  filterUsers() {
    if (!this.userSearchText.trim()) {
      this.filteredUsers = [...this.users];
    } else {
      const searchText = this.userSearchText.toLowerCase().trim();
      this.filteredUsers = this.users.filter(user => 
        (user.userName && user.userName.toLowerCase().includes(searchText)) || 
        (user.name && user.name.toLowerCase().includes(searchText)) || 
        (user.emailAddress && user.emailAddress.toLowerCase().includes(searchText))
      );
    }
  }
  
  getUserNameById(userId: number): string {
    if (!userId || !this.users || this.users.length === 0) {
      return '';
    }
    
    const user = this.users.find(u => u.id === userId);
    return user ? (user.userName || user.name || '') : '';
  }
  
  displayUserFn(user: any): string {
    return user && user.userName ? user.userName : '';
  }
  
  onUserSelected(event: MatAutocompleteSelectedEvent): void {
    const selectedUser = event.option.value;
    if (selectedUser && selectedUser.id) {
      this.userPunishment.userId = selectedUser.id;
    }
  }

  onDateChange() {
    if (this.userPunishment) {
      this.updateDateFromSelectedDate();
    }
  }

  onDateBlur(event: FocusEvent) {
    const input = event.target as HTMLInputElement;
    const value = input.value.trim();
    
    if (!value) {
      this.selectedDate = null;
      this.userPunishment.dateAt = '';
      return;
    }

    const dateMoment = moment(value, ['DD/MM/YYYY', 'D/M/YYYY'], true);
    
    if (dateMoment.isValid()) {
      this.selectedDate = dateMoment.toDate();
      this.updateDateFromSelectedDate();
    } else {
      this.selectedDate = null;
      this.userPunishment.dateAt = '';
    }
  }

  private updateDateFromSelectedDate() {
    if (this.selectedDate) {
      this.userPunishment.dateAt = moment(this.selectedDate).format('YYYY-MM-DD');
    } else {
      this.userPunishment.dateAt = '';
    }
  }

  save() {
    if (this.isSaving) {
      return;
    }

    this.isSaving = true;
    
    this.updateDateFromSelectedDate();
    
    if (!this.selectedDate || !this.userPunishment.dateAt) {
      this.notify.warn('Please enter a valid date');
      this.isSaving = false;
      return;
    }
    
    let request;
    if (this.isEditMode) {
      const updateDto = this.userPunishment as UpdateUserPunishmentDto;
      request = this._userPunishmentService.update(updateDto);
    } else {
      const createDto = this.userPunishment as CreateUserPunishmentDto;
      request = this._userPunishmentService.create(createDto);
    }

    request.pipe(
      finalize(() => {
        this.isSaving = false;
      })
    ).subscribe(
      (response) => {
        if (response.success) {
          this.notify.success(
            this.isEditMode
              ? 'User punishment updated successfully'
              : 'User punishment created successfully'
          );
          this._dialogRef.close(response.result);
        }
      },
      (error) => {
        console.error('Error saving user punishment:', error);
        if (error && error.error && error.error.error && error.error.error.message) {
          this.notify.error(error.error.error.message);
        } else {
          this.notify.error('An error occurred while saving the user punishment');
        }
      }
    );
  }

  ngAfterViewInit(): void {
    Promise.resolve().then(() => {
      if (this.createForm) {
        for (const key in this.createForm.controls) {
          if (this.createForm.controls.hasOwnProperty(key)) {
            this.createForm.controls[key].markAsTouched();
          }
        }
      }
    });
  }

  close(result?: any): void {
    this._dialogRef.close(result);
  }
}
