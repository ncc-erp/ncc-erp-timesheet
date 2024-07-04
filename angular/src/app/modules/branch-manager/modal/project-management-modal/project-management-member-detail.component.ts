import {Component, Inject} from '@angular/core';
import {MAT_DIALOG_DATA} from '@angular/material/dialog';
import {ProjectTargetUserDto} from '@app/modules/branch-manager/Dto/branch-manage-dto';
import {ManageUserForBranchService} from '@app/service/api/manage-user-for-branch.service';
import {OnInit} from '@node_modules/@angular/core';
import {ProjectUserDto} from '@shared/paged-listing-component-base';
import {IProjectTargetUser} from '@app/modules/branch-manager/modal/project-management-modal/interface/project-type.interface';

@Component({
    selector: 'app-project-management-member-detail',
    templateUrl: './project-management-member-detail.component.html',
    styleUrls: ['./project-management-member-detail.component.css']
})
export class ProjectManagementMemberDetailComponent implements OnInit {
    projectItem: ProjectTargetUserDto[] = [];
    constructor(
        private manageUserForBranchService: ManageUserForBranchService,
        @Inject(MAT_DIALOG_DATA) public data: { projectItem: IProjectTargetUser },
    ) {}

    ngOnInit() {
      this.manageUserForBranchService.getAllUserInProject(
          this.data.projectItem.projectId,
          this.data.projectItem.startDate,
          this.data.projectItem.endDate,
          ProjectUserDto
      ).subscribe((res) => {
        this.projectItem = res.result;
      });
    }
}
