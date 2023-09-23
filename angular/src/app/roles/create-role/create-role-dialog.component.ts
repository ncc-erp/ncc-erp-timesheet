import { Component, Injector, OnInit } from '@angular/core';
import { MatDialogRef, MatCheckboxChange } from '@angular/material';
import { finalize } from 'rxjs/operators';
import * as _ from 'lodash';
import { AppComponentBase } from '@shared/app-component-base';
import {
  RoleServiceProxy,
  RoleDto,
  ListResultDtoOfPermissionDto,
  PermissionDto,
  CreateRoleDto
} from '@shared/service-proxies/service-proxies';

@Component({
  templateUrl: 'create-role-dialog.component.html',
  styles: [
    `
      mat-form-field {
        width: 100%;
      }
      mat-checkbox {
        padding-bottom: 5px;
      }
    `
  ]
})
export class CreateRoleDialogComponent extends AppComponentBase
  implements OnInit {
  saving = false;
  role: RoleDto = new RoleDto();
  permissions: PermissionDto[] = [];
  grantedPermissionNames: string[] = [];
  checkedPermissionsMap: { [key: string]: boolean } = {};
  defaultPermissionCheckedStatus = true;

  constructor(
    injector: Injector,
    private _roleService: RoleServiceProxy,
    private _dialogRef: MatDialogRef<CreateRoleDialogComponent>
  ) {
    super(injector);
  }

  ngOnInit(): void {
   
  }

  save(): void {
    this.saving = true;

    const role_ = new CreateRoleDto();
    role_.init(this.role);

    this._roleService
      .create(role_)
      .pipe(
        finalize(() => {
          this.saving = false;
        })
      )
      .subscribe(() => {
        this.notify.success(this.l('Saved Successfully'));
        this.close(true);
      });
  }

  close(result: any): void {
    this._dialogRef.close(result);
  }
}
