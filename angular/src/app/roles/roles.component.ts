import { Component, Injector } from '@angular/core';
import { MatDialog } from '@angular/material';
import { PERMISSIONS_CONSTANT } from '@app/constant/permission.constant';
import { appModuleAnimation } from '@shared/animations/routerTransition';
import {
    PagedListingComponentBase,
    PagedRequestDto
} from '@shared/paged-listing-component-base';
import {
    PagedResultDtoOfRoleDto, RoleDto, RoleServiceProxy
} from '@shared/service-proxies/service-proxies';
import { finalize } from 'rxjs/operators';
import { CreateRoleDialogComponent } from './create-role/create-role-dialog.component';

class PagedRolesRequestDto extends PagedRequestDto {
    keyword: string;
}

@Component({
    templateUrl: './roles.component.html',
    animations: [appModuleAnimation()],
    styles: [
        `
          mat-form-field {
            padding: 10px;
          }
        `
    ]
})
export class RolesComponent extends PagedListingComponentBase<RoleDto> {
    ADD_ROLE = PERMISSIONS_CONSTANT.AddRole;
    DELETE_ROLE = PERMISSIONS_CONSTANT.DeleteRole;
    EDIT_ROLE = PERMISSIONS_CONSTANT.EditRole;
    VIEW_DETAIL_ROLE = PERMISSIONS_CONSTANT.ViewDetailRole;
    roles: RoleDto[] = [];
    keyword = '';

    constructor(
        injector: Injector,
        private _rolesService: RoleServiceProxy,
        private _dialog: MatDialog
    ) {
        super(injector);
    }

    list(
        request: PagedRolesRequestDto,
        pageNumber: number,
        finishedCallback: Function
    ): void {
        request.keyword = this.keyword;

        this._rolesService
            .getAll(request.keyword, request.skipCount, request.maxResultCount)
            .pipe(
                finalize(() => {
                    finishedCallback();
                })
            )
            .subscribe((result: PagedResultDtoOfRoleDto) => {
                this.roles = result.items;
                this.showPaging(result, pageNumber);
            });
    }

    createRole(): void {
        this.showCreateOrEditRoleDialog();
    }

    showCreateOrEditRoleDialog(id?: number): void {
        let createOrEditRoleDialog;
        createOrEditRoleDialog = this._dialog.open(CreateRoleDialogComponent);
        createOrEditRoleDialog.afterClosed().subscribe(result => {
            if (result) {
                this.refresh();
            }
        });
    }

    delete(role: RoleDto): void {

        abp.message.confirm(
            this.l('Delete role ' + role.displayName),
            (result: boolean) => {
                if (result) {
                    this._rolesService
                        .delete(role.id)
                        .pipe(
                            finalize(() => { })
                        )
                        .subscribe(() => {
                            abp.notify.success(this.l('Deleted successfully'));
                            this.refresh();
                        });
                }
            }
        );
    }
}
