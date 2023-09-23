import { QuantityProjectDto } from './../../service/api/model/project-Dto';
import { ProjectManagerService } from './../../service/api/project-manager.service';
import { Component, OnInit, Injector } from '@angular/core';
import { CreateProjectComponent } from './create-project/create-project.component';
import { MatDialog } from '@angular/material';
import { ProjectDetailComponent } from './project-detail/project-detail.component';
import { AppComponentBase } from '@shared/app-component-base';
import * as _ from 'lodash';
import { CustomerService } from '@app/service/api/customer.service';
import { GetProjectDto, ProjectDto } from '@app/service/api/model/project-Dto';
import * as moment from 'moment';
import { PERMISSIONS_CONSTANT } from '@app/constant/permission.constant';

@Component({
  selector: 'app-project-manager',
  templateUrl: './project-manager.component.html',
  styleUrls: ['./project-manager.component.less']
})
export class ProjectManagerComponent extends AppComponentBase implements OnInit {
  ADD_PROJECT = PERMISSIONS_CONSTANT.AddProject;
  EDIT_PROJECT = PERMISSIONS_CONSTANT.EditProject;
  DELETE_PROJECT = PERMISSIONS_CONSTANT.DeleteProject;
  CHANGE_STATUS_PROJECT = PERMISSIONS_CONSTANT.ChangeStatusProject;
  VIEW_DETAIL_PROJECT = PERMISSIONS_CONSTANT.ViewDetailProject;

  isTableLoading = false;
  allProjects = [] as GetProjectDto[];
  mapProjects = [];
  isProjectAdmin: boolean;
  projectAd: any;
  searchKey = '';
  status = 0;
  activeCount = 0;
  deactiveCount = 0;
  projectTypes = this.APP_CONSTANT.EnumProjectType;
  isAdmin: boolean;
  contextMenuPosition = { x: '0px', y: '0px' };
  customers = [];
  constructor(
    private projectManagerService: ProjectManagerService,
    private customerService: CustomerService,
    private _dialog: MatDialog,
    injector: Injector
  ) {
    super(injector);
  }
  onContextMenu(event: MouseEvent, temp) {
    event.preventDefault();
    this.contextMenuPosition.x = event.clientX + 'px';
    this.contextMenuPosition.y = event.clientY + 'px';
    temp.openMenu();
  }
  ngOnInit() {
    this.isAdmin = this.permission.isGranted("Project");    
    this.getProjects();
    this.getCustomers();
  }

  getCustomers() {
    this.customerService.getAllCustomer().subscribe(res => {
      this.customers = res.result;

    })
  }

  getProjects() {
    this.isTableLoading = true;
    this.projectManagerService.getAll(this.status, this.searchKey).subscribe(res => {
      this.allProjects = res.result;
      this.mapProjects = this.buildData(this.allProjects); 
      this.isTableLoading = false;
    });
    this.getQuantityProject();
  }
  
  getQuantityProject(){
    this.projectManagerService.getQuantityProject().subscribe(res => {
      let listQuantityProject: QuantityProjectDto[] = [];
      listQuantityProject = res.result;
      listQuantityProject.forEach((project) => {
        if (project.status == this.APP_CONSTANT.EnumProjectStatus.Active) {
          this.activeCount = project.quantity
        }
        else if (project.status == this.APP_CONSTANT.EnumProjectStatus.Deactive) {
          this.deactiveCount = project.quantity
        }
      })

    })
  }


  buildData(data: Array<any>) {
    return _(data)
      .groupBy(x => x.customerName)
      .map((value, key) => ({ customerName: key, items: value }))
      .value();
  }

  deactiveProject(project): void {
    abp.message.confirm(
      "Deactive project: '" + project.name + "'?",
      (result: boolean) => {
        if (result) {
          let param = { id: project.id }
          this.projectManagerService.deactiveProject(param).subscribe(() => {
            this.notify.success(this.l('Active Project Successfully'));
            this.getProjects();
          });
        }
      }
    );
  }

  formatDate(time): string {
    return moment(time).format("DD/MM/YYYY");
  }

  activeProject(project): void {
    abp.message.confirm(
      "Active project: '" + project.name + "'?",
      (result: boolean) => {
        if (result) {
          let param = { id: project.id }
          this.projectManagerService.activeProject(param).subscribe(() => {
            this.notify.success(this.l('Active Project Successfully'));
            this.getProjects();
          });
        }
      }
    );
  }

  deleteProject(project): void {
    abp.message.confirm(
      "Delete project: '" + project.name + "'?",
      (result: boolean) => {
        if (result) {
          this.projectManagerService.deleteProject(project.id).subscribe(() => {
            this.notify.success(this.l('Delete Project Successfully'));
            this.getProjects();
          });
        }
      }
    );
  }

  createProject(): void {
    let project = {} as ProjectDto;
    this.showCreateOrEditProjectDialog(project);
  }

  editProject(project): void {
    this.projectManagerService.getProject(project.id).subscribe(res => {
      project = res.result as ProjectDto;
      this.showCreateOrEditProjectDialog(project);
    });
  }
  

  showCreateOrEditProjectDialog(project: ProjectDto): void {    
    let dialogRef = this._dialog.open(CreateProjectComponent, {
      disableClose : true,
      data: {project: project, customers: this.customers},
      width: "1350px"
    }
    )
    dialogRef.afterClosed().subscribe((res) => {
      if(res) {
        this.getProjects();
      }
    }
    );
  }

  showProjectDetailDialog(project): void {
    // let item = project.
    let dialogRef = this._dialog.open(ProjectDetailComponent, {
      minWidth: '450px',
      width: '800px',
      data: project
    })
    // this.projectManagerService.getProjectDetailTask(project.id)
    dialogRef.afterClosed().subscribe(() => {
      // this.getAll();
    }
    );
  }

}
