import {Component, Inject} from '@angular/core';
import {MAT_DIALOG_DATA} from '@angular/material/dialog';
import {ManageUserForBranchService} from '@app/service/api/manage-user-for-branch.service';
import {OnInit} from '@node_modules/@angular/core';
import {PageProjectUserDto} from '@shared/paged-listing-component-base';
import {IProjectTargetUser} from '@app/modules/branch-manager/modal/project-management-modal/interface/project-type.interface';
import {ESortMemberEffort, ESortType} from '@app/modules/branch-manager/modal/project-management-modal/enum/sort-member-effort.enum';

@Component({
    selector: 'app-project-management-member-detail',
    templateUrl: './project-management-member-detail.component.html',
    styleUrls: ['./project-management-member-detail.component.css']
})
export class ProjectManagementMemberDetailComponent implements OnInit {
    totalItems: number;
    projectItem: IProjectTargetUser[] = [];
    sortType = ESortType.Member;
    sortMemberOrEffort = ESortMemberEffort;
    currentSortMember: ESortMemberEffort = ESortMemberEffort.UP_MEMBER;
    currentSortEffort: ESortMemberEffort = ESortMemberEffort.UP_EFFORT;
    constructor(
        private manageUserForBranchService: ManageUserForBranchService,
        @Inject(MAT_DIALOG_DATA) public data: { projectItem: IProjectTargetUser },
    ) {}

    ngOnInit() {
        const request = new PageProjectUserDto();
       this.manageUserForBranchService.getAllUserInProject(this.data.projectItem.projectId, this.data.projectItem.startDate,
            this.data.projectItem.endDate, request).subscribe((res) => {
        this.projectItem = res.result;
        this.totalItems = res.result.length;
      });
    }

    toggleSortOrder(click: boolean) {
        if (click === true) {
            this.sortType = ESortType.Member;
            if (this.currentSortMember === ESortMemberEffort.UP_MEMBER) {
                this.currentSortMember = ESortMemberEffort.DOWN_MEMBER;
            } else {
                this.currentSortMember = ESortMemberEffort.UP_MEMBER;
            }
        } else  {
            this.sortType = ESortType.Effort;
            if (this.currentSortEffort === ESortMemberEffort.UP_EFFORT) {
                this.currentSortEffort = ESortMemberEffort.DOWN_EFFORT;
            } else {
                this.currentSortEffort = ESortMemberEffort.UP_EFFORT;
            }
        }
    }
}
