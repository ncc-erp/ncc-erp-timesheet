import { PERMISSIONS_CONSTANT } from '@app/constant/permission.constant';
import { ConfigurationService } from './../../../service/api/configuration.service';
import { Component, OnInit, Injector, Inject } from '@angular/core';
import { AppComponentBase } from '@shared/app-component-base';
import { RoleDto } from '@shared/service-proxies/service-proxies';
import { MatCheckboxChange, MAT_DIALOG_DATA, MatDialogRef, MAT_DATE_LOCALE } from '@angular/material';
import * as _ from 'lodash';
import { UserService } from '@app/service/api/user.service';

@Component({
  selector: 'app-role-user',
  templateUrl: './role-user.component.html',
  providers: [
    { provide: MAT_DATE_LOCALE, useValue: 'en-GB' }
  ]
})
export class RoleUserComponent extends AppComponentBase implements OnInit {
  title = '';
  saving = false;
  ischeck = false;
  user = {} as UpdateRoleUserDTO;
  roles: RoleDto[] = [];
  checkedRolesMap: { [key: string]: boolean } = {};
  users;
  userId: number;
  isSaving: boolean = false;
  constructor(
    injector: Injector,
    @Inject(MAT_DIALOG_DATA) public data: any,
    private diaLogRef: MatDialogRef<RoleUserComponent>,
    private userService: UserService,
    private configService: ConfigurationService
  ) {
    super(injector);
    this.userId = data.userId;
  }

  ngOnInit(): void {
    this.getRole()
  }
  getRole(){
    this.userService.getOneUser(this.userId).subscribe(result => {
      this.user = result.result;
      this.title = 'Edit User Role ' + this.user.userName;
      this.userService.getRoles().subscribe(data => {
        this.roles = data.result.items;
        this.setInitialRolesStatus();
      });
    });
  }
 
  setInitialRolesStatus(): void {
    if (this.user.id != null) {
      this.user.roleNames = _.map(this.user.roleNames, (o) => { return o.toUpperCase() });
    }
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
    _.forEach(this.checkedRolesMap, function (value, key) {
      if (value) {
        roles.push(key);
      }
    });
    return roles;
  }

  save(): void {
    this.isSaving = true;
    let updateRoleUser = {
      id: this.user.id,
      roleNames: this.user.roleNames
    } as UpdateRoleUserDTO;

    this.userService.updateRole(updateRoleUser).subscribe(data => {
      if (data.success) {
        this.notify.success(this.l("Update User Role Successfully!"));
        this.close(true);
        this.isSaving = false;
      }
    },
      err => {
        this.isSaving = false;
      }
    );
  }
  close(isCloseDialog): void {
    this.diaLogRef.close(isCloseDialog);
  }
}

export class UpdateRoleUserDTO {
  roleNames: string[];
  id: number;
  userName: string;
}
