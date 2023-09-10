import { Component, Injector, OnInit, Inject } from '@angular/core';
import { MatCheckboxChange, MAT_DIALOG_DATA } from '@angular/material';
import * as _ from 'lodash';
import { AppComponentBase } from '@shared/app-component-base';
import {
  UserServiceProxy,
  UserDto,
  RoleDto
} from '@shared/service-proxies/service-proxies';

@Component({
  templateUrl: './create-edit-user-dialog.component.html',
  styles: ['./create-edit-user-dialog.component.css']
})
export class CreateEditUserComponent extends AppComponentBase
  implements OnInit {
  activeRandom = true;
  shPassword = false;
  shConfirmPassword = false;
  confirmPassword = '';
  confirmDisale = false;

  title = '';
  saving = false;
  ischeck = false;
  user = {} as UserDto;
  roles: RoleDto[] = [];
  checkedRolesMap: { [key: string]: boolean } = {};
  defaultRoleCheckedStatus = false;

  constructor(
    injector: Injector,
    public _userService: UserServiceProxy,
    @Inject(MAT_DIALOG_DATA) public data: any
  ) {
    super(injector);
  }

  ngOnInit(): void {
   const users = _.cloneDeep(this.data.user);
    this.user = users as UserDto;
    this.title = this.user.name != undefined ? 'Edit User' : 'Create new User';
    this.roles = this.data.roles;
    this.setInitialRolesStatus();
    if (!this.user.id) {
      this.checkedRolesMap['BASICUSER'] = true;
      this.user.roleNames = ['BASICUSER'];
      this.user.isActive = true;
    }
    this.ischeck = this.user.roleNames.length === 0;
  }

  setInitialRolesStatus(): void {
    _.map(this.roles, item => {
      this.checkedRolesMap[item.normalizedName] = this.isRoleChecked(item.normalizedName);
    });
  }
  isRoleChecked(val: string): boolean {
    return _.includes(this.user.roleNames, val);
  }

  onRoleChange(role: RoleDto, $event: MatCheckboxChange) {
    this.checkedRolesMap[role.normalizedName] = $event.checked;
    this.user.roleNames = this.getCheckedRoles();
    this.ischeck = this.user.roleNames.length === 0;
    
  }

  getCheckedRoles(): string[] {
    const roles: string[] = [];
    _.forEach(this.checkedRolesMap, function(value, key) {
      if (value) {
        roles.push(key);
      }
    });
    return roles;
  }

  randomPassword() {
    let passRandom = Math.random()
      .toString(36)
      .substr(2, 10);

    this.user.password = passRandom;
    this.confirmPassword = this.user.password;
    this.reactChangePassword(true);
  }
  backField(val) {
    if (val === '') {
      this.confirmPassword = '';
      this.reactChangePassword(false);
    }
  }
  reactChangePassword(val: boolean) {
    this.confirmDisale = val;
    this.shPassword = val;
  }
}
