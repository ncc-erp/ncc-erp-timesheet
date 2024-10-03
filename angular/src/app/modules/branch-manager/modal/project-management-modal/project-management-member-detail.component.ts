import {Component, Inject, Injector} from '@angular/core';
import {MAT_DIALOG_DATA, MatDialogRef} from '@angular/material/dialog';
import {ManageUserForBranchService} from '@app/service/api/manage-user-for-branch.service';
import {OnInit} from '@node_modules/@angular/core';
import {PageProjectUserDto} from '@shared/paged-listing-component-base';
import {IProjectTargetUser} from '@app/modules/branch-manager/modal/project-management-modal/interface/project-type.interface';
import {ESortMemberEffort, ESortType} from '@app/modules/branch-manager/modal/project-management-modal/enum/sort-member-effort.enum';
import { UserTypeDto } from '../../Dto/branch-manage-dto';
import { AppComponentBase } from '@shared/app-component-base';

@Component({
    selector: 'app-project-management-member-detail',
    templateUrl: './project-management-member-detail.component.html',
    styleUrls: ['./project-management-member-detail.component.css']
})
export class ProjectManagementMemberDetailComponent extends AppComponentBase implements OnInit {
    totalItems: number;
    projectItem: IProjectTargetUser[] = [];
    sortType = ESortType.Member;
    sortMemberOrEffort = ESortMemberEffort;
    currentSortMember: ESortMemberEffort = ESortMemberEffort.UP_MEMBER;
    currentSortEffort: ESortMemberEffort = ESortMemberEffort.UP_EFFORT;
    isLoading = true;
    constructor(
        injector: Injector,
        private manageUserForBranchService: ManageUserForBranchService,
        public dialogRef: MatDialogRef<ProjectManagementMemberDetailComponent>,
        @Inject(MAT_DIALOG_DATA) public data: { projectItem: IProjectTargetUser },
    ) {
        super(injector);
    }

    ngOnInit() {
        const request = new PageProjectUserDto();
       this.manageUserForBranchService.getAllUserInProject(this.data.projectItem.branchId, this.data.projectItem.projectId, this.data.projectItem.startDate,
            this.data.projectItem.endDate, request).subscribe((res) => {
        this.projectItem = res.result;
        this.totalItems = res.result.length;
        this.isLoading = false;
      });
    }

    toggleSortOrder(click: boolean) {
        if (click) {
            this.sortType = ESortType.Member;
            this.currentSortMember = (this.currentSortMember === ESortMemberEffort.UP_MEMBER) ? ESortMemberEffort.DOWN_MEMBER : ESortMemberEffort.UP_MEMBER;
        } else {
            this.sortType = ESortType.Effort;
            this.currentSortEffort = (this.currentSortEffort === ESortMemberEffort.UP_EFFORT) ? ESortMemberEffort.DOWN_EFFORT : ESortMemberEffort.UP_EFFORT;
        }
    }

    onCancelClick(): void {
        this.dialogRef.close();
    }

    onProjectItemChange(value: number, index: number): void {
        this.projectItem[index].userType = value;
        console.log(this.projectItem);
    }

    onSaveClick(): void {
        this.isLoading = true;
        const payload: UserTypeDto[] = [];
        this.projectItem.forEach((item) => {
            payload.push({projectUserId: item.projectUserId, userType: item.userType as number});
        });
        this.manageUserForBranchService.updateTypeOfUsersInProject({ userTypes: payload, projectId: this.data.projectItem.projectId }).subscribe(() => {
            this.dialogRef.close(true);
        });
    }
}
