import { Component, Inject, Injector, OnInit } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material';
import { projectList } from '../detail-participating-projects.component';
import { AppComponentBase } from '@shared/app-component-base';
import { ManageUserProjectForBranchService } from '@app/service/api/manage-user-project-for-branch.service';

@Component({
  selector: 'app-popup-update-project',
  templateUrl: './popup-update-project.component.html',
  styleUrls: ['./popup-update-project.component.css']
})
export class PopupUpdateProjectComponent extends AppComponentBase implements OnInit {
  project = {} as projectList;
  saving = false;
  constructor(
    injector: Injector,
    private _dialogRef: MatDialogRef<PopupUpdateProjectComponent>,
    @Inject(MAT_DIALOG_DATA) public data: any,
    private manageUserProjectForBranchService: ManageUserProjectForBranchService,
  ) {
    super(injector);
    
  }

  ngOnInit() {
    this.project = this.data.project;
  }

  submit(){
    const data = {
      userId: this.data.userId,
      projectId: this.project.projectId,
      type: this.project.valueOfUserType,
      shadowPercentage: this.project.shadowPercentage,
    }
    this.manageUserProjectForBranchService.createValueOfUser(data).subscribe(result => {
      this.close(true);
    })
  }

  close(result: any, data?: any): void {
    this._dialogRef.close({result, data});
  }

}
