import { ProjectUserType } from './../../user/user.component';
import { TaskService } from './../../../service/api/task.service';
import { Component, OnInit, Optional, Injector, Inject, ViewChild, ChangeDetectionStrategy, Pipe, PipeTransform } from '@angular/core';
import { ProjectManagerService } from './../../../service/api/project-manager.service';
import { MAT_DIALOG_DATA, MatDialogRef, MatDialog, MatCheckboxChange, MAT_DATE_LOCALE } from '@angular/material';
import { AppComponentBase } from '@shared/app-component-base';
import { CustomerService } from '../../../service/api/customer.service';
import { MemberService } from '../../../service/api/member.service';
import { CreateEditCustomerComponent } from '@app/modules/customer/create-edit-customer/create-edit-customer.component';
import * as _ from 'lodash';
import { CustomerDto } from '@app/modules/customer/customer.component';
import { APP_CONSTANT } from '@app/constant/api.constants';
import { TaskDto } from '@app/modules/task/task.component';
import { AppConsts } from '@shared/AppConsts';
import { ProjectDto, ProjectUserDto, ProjectTaskDto, DisplayProjectTargetUserDto, TaskProjectDto, UserDto } from '@app/service/api/model/project-Dto';
import { BranchDto } from '@shared/service-proxies/service-proxies';
import { FormControl } from '@angular/forms';
import { BranchService } from '@app/service/api/branch.service';
import { PERMISSIONS_CONSTANT } from '@app/constant/permission.constant';

@Component({
  changeDetection: ChangeDetectionStrategy.OnPush,
  selector: 'app-create-project',
  templateUrl: './create-project.component.html',
  styleUrls: ['./create-project.component.css']
})
export class CreateProjectComponent extends AppComponentBase implements OnInit {

  project = { users: [], tasks: [], projectTargetUsers: [], isAllUserBelongTo: true } as ProjectDto;
  // formCreateEdit: FormGroup;
  title: string;

  // allMembers: UserDto[] = [];
  displayProjectMembers: ProjectUserDto[] = [];
  projectMembers: ProjectUserDto[] = [];
  activeMembers: UserDto[] = [];
  displayActiveMembers: UserDto[] = [];

  listCustomer: CustomerDto[] = [];
  customerSearch: FormControl = new FormControl("")
  listCustomerFilter: CustomerDto[];

  EditTeamWorkType = PERMISSIONS_CONSTANT.EditTypeWork;

  projectTasks: ProjectTaskDto[] = [];
  availableTask: TaskDto[] = [];

  projectTypes = this.APP_CONSTANT.EnumProjectType;

  searchTeamApiInput: Function;

  checkAllTaskStatus: number;
  searchProjectMemberText = '';
  searchMemberText = '';
  searchTargetUserText = '';
  isShowDeactiveMember = false;
  // User these arr to keep save data of target users in the processing because projectTargetUsers doesn't have 'name' property
  projectTargetUsers: DisplayProjectTargetUserDto[] = [];
  availableTargetUsers: UserDto[] = [];
  displayAvailableTargetUsers: UserDto[] = [];
  isShowInactiveUser = false;

  respone = 0;

  isSaving: boolean = false;
  userTypes = [
    { value: 0, label: "Staff" },
    { value: 1, label: "Internship" },
    { value: 2, label: "Collaborator" }
  ];

  status = [
    { value: "", label: "All" },
    { value: true, label: "Active" },
    { value: false, label: "InActive" }
  ];

  userTypeForFilter = -1;
  userBranchForFilter: number = 0;
  userLevelForFilter = -1;
  statusForFilter;

  listBranch: BranchDto[] = [];
  branchSearch: FormControl = new FormControl("")
  listBranchFilter: BranchDto[];

  isShowTeamMember = false;
  constructor(
    injector: Injector,
    private projectManagerService: ProjectManagerService,
    private memberService: MemberService,
    private customerService: CustomerService,
    private taskService: TaskService,
    private _dialogRef: MatDialogRef<CreateProjectComponent>,
    private _dialog: MatDialog,
    private branchService: BranchService,
    @Optional() @Inject(MAT_DIALOG_DATA) public data: any
  ) {
    super(injector);
    // this.userBranchForFilter = this.appSession.user.branchId;
    this.branchSearch.valueChanges.subscribe(() => {
      this.filterBranch();
    });
    this.customerSearch.valueChanges.subscribe(() => {
      this.filterCustomer();
    });
  }
  ngOnInit() {
    this.project = this.data.project;
    //this.listCustomer = this.data.customers;
    this.getTasks();
    this.getAllMember();
    this.getListBranch();
    this.getAllCustomer();
    if (this.project.id == null) {
      this.title = 'Create Project';
      this.project.projectType = APP_CONSTANT.EnumProjectType.Fixedfee;
    }
    else {
      this.title = 'Edit Project :';
    }
    this.statusForFilter = "";

  }

  getListBranch() {
    this.branchService.getAllBranchFilter(true).subscribe(res => {
      this.listBranch = res.result;
      this.listBranchFilter = this.listBranch;
    });
  }

  filterBranch(): void {
    if (this.branchSearch.value) {
      this.listBranch = this.listBranchFilter.filter(data => data.displayName.toLowerCase().includes(this.branchSearch.value.toLowerCase().trim()));
    } else {
      this.listBranch = this.listBranchFilter.slice();
    }
  }
  showTeamMember(){
    this.isShowTeamMember = !this.isShowTeamMember;
  } 

  getTasks() {
    this.taskService.getAll().subscribe(res => {
      const allTasks = res.result as TaskDto[];
      if (this.project.tasks && this.project.tasks.length > 0) {
        this.availableTask = allTasks.filter(t => !t.isDeleted && !this.project.tasks.some(s => t.id === s.taskId));
      } else {
        this.availableTask = allTasks.filter(t => !t.isDeleted);
      }
      if (!this.project.id) {
        for (let i = 0; i < allTasks.length; i++) {
          const element = allTasks[i];
          if (element.type === APP_CONSTANT.EnumTaskType.Commontask) {
            const el = {
              billable: true,
              name: element.name,
              taskId: element.id,
              type: element.type
            }
            this.projectTasks.push(el);
            this.availableTask.splice(this.availableTask.indexOf(element), 1);
          }
        }
      } else {
        this.project.tasks.forEach(t => {
          const task = { taskId: t.taskId, billable: t.billable } as ProjectTaskDto;
          const item = allTasks.find(s => s.id === task.taskId);
          task.name = item ? item.name : '';
          task.type = item ? item.type : -1;
          this.projectTasks.push(task);
        });
      }
      this.updateCheckAllStatus()
    })
  }

  getAllMember() {
    this.memberService.getUserNoPagging().subscribe(res => {
      const allMembers = res.result as UserDto[];
      if (this.project.users) {
        this.activeMembers = allMembers.filter(member => member.isActive && !this.project.users.some(s => member.id === s.userId));
      } else this.activeMembers = allMembers.filter(member => member.isActive);

      if (this.project.projectTargetUsers) {
        this.availableTargetUsers = allMembers.filter(member => !this.project.projectTargetUsers.some(s => member.id === s.userId));
      } else
        this.availableTargetUsers = allMembers;
    
      // tslint:disable-next-line: max-line-length
      if (this.project.users && this.project.users.length > 0) {
        this.project.users.map(s => {
          const item = { userId: s.userId, ptype: s.type, isTemp: s.isTemp } as ProjectUserDto;
          const user = allMembers.find(u => u.id === item.userId);
          if (user) {
            item.type = user.type;
            item.isActive = user.isActive;
            item.jobTitle = user.jobTitle;
            item.level = user.level;
            item.name = user.name;
            item.avatarFullPath = user.avatarFullPath;
            item.branch = user.branch;
            item.emailAddress = user.emailAddress;
            item.branchColor = user.branchColor;
            item.branchDisplayName = user.branchDisplayName;
            this.projectMembers.push(item);
            if (this.isShowDeactiveMember || item.ptype != APP_CONSTANT.EnumUserType.DeActive) {
              this.displayProjectMembers.push(item)
            }
          }
        });
      }
      if (this.project.projectTargetUsers && this.project.projectTargetUsers.length > 0) {
        this.project.projectTargetUsers.map(s => {
          const item = { userId: s.userId, roleName: s.roleName } as DisplayProjectTargetUserDto;
          const user = allMembers.find(u => u.id === item.userId);
          if (user) {
            item.type = user.type;
            item.isActive = user.isActive;
            item.jobTitle = user.jobTitle;
            item.level = user.level;
            item.name = user.name;
            item.avatarFullPath = user.avatarFullPath;
            item.branch = user.branch;
            item.emailAddress = user.emailAddress;
            item.branchColor = user.branchColor;
            item.branchDisplayName = user.branchDisplayName;
          }
          this.projectTargetUsers.push(item);
        });
      }

      this.displayActiveMembers = this.activeMembers.filter(x => true); // initial value of displayActive... list (call only 1 time when OnInint)
      this.displayAvailableTargetUsers = this.availableTargetUsers.filter(x => true); // initial value of displayAvailable... list (call only 1 time when OnInint)

    })

  }

  onShowDeactiveMemberChange() {
    if (this.isShowDeactiveMember) {

      this.displayProjectMembers = this.projectMembers.slice().filter(
        member => ((member.name.toLowerCase().includes(this.searchProjectMemberText.toLowerCase()))
          || (member.emailAddress.toLowerCase().includes(this.searchProjectMemberText.toLowerCase()))
        )
      )

    } else {

      this.displayProjectMembers = this.projectMembers.filter(
        member => member.ptype !== APP_CONSTANT.EnumUserType.DeActive).filter(
          member => ((member.name.toLowerCase().includes(this.searchProjectMemberText.toLowerCase()))
            || (member.emailAddress.toLowerCase().includes(this.searchProjectMemberText.toLowerCase()))
          )
        )
    }
  }

  filterProjectMember() {
    this.displayProjectMembers = this.projectMembers.filter(
      member => (((member.name.toLowerCase().includes(this.searchProjectMemberText.toLowerCase()))
        || (member.emailAddress.toLowerCase().includes(this.searchProjectMemberText.toLowerCase()))))
    );

    if (this.isShowInactiveUser && this.isShowDeactiveMember) {
      this.displayProjectMembers = this.displayProjectMembers.filter(
        member => (
          member.isActive === APP_CONSTANT.EnumUserStatus.InActive
          && member.ptype === APP_CONSTANT.EnumUserType.DeActive)
      );
      return;
    }

    if (this.isShowInactiveUser) {
      this.displayProjectMembers = this.displayProjectMembers.filter(
        member => (
          member.isActive === APP_CONSTANT.EnumUserStatus.InActive)
      );
      return;
    }

    if (this.isShowDeactiveMember) {
      this.displayProjectMembers = this.displayProjectMembers.filter(
        member => (member.ptype === APP_CONSTANT.EnumUserType.DeActive)
      );
      return;
    }

    this.displayProjectMembers = this.displayProjectMembers.filter(
      member => (member.ptype !== APP_CONSTANT.EnumUserType.DeActive)
    );
    return;
  }

  removeMemberFromProject(member: ProjectUserDto, index) {
    // this.projectMembers.splice(index, 1);
    this.displayProjectMembers.splice(index, 1);
    this.projectMembers.splice(this.projectMembers.findIndex(s => s.userId === member.userId), 1);
    const { userId, isActive, jobTitle, level, name, type, avatarFullPath, branch, emailAddress, branchColor, branchDisplayName } = member;
    const u = {
      id: userId,
      isActive,
      jobTitle,
      level,
      name,
      type,
      avatarFullPath,
      branch,
      emailAddress,
      branchColor,
      branchDisplayName,
    } as UserDto;
    this.activeMembers.push(u);
    this.displayActiveMembers.push(u);
  }

  searchMember() {
    this.displayActiveMembers = this.activeMembers.filter(
      member => (!this.searchMemberText || (member.emailAddress.toLowerCase().includes(this.searchMemberText.toLowerCase())) || member.name.search(new RegExp(this.searchMemberText, 'ig')) > -1)
        && (this.userBranchForFilter == 0 || member.branchId === this.userBranchForFilter)
        && (this.userTypeForFilter < 0 || member.type === this.userTypeForFilter)
    );
  }

  searchProjectMember() {
    this.displayProjectMembers = this.projectMembers.filter(
      member => (((member.name.toLowerCase().includes(this.searchProjectMemberText.toLowerCase()))
        || (member.emailAddress.toLowerCase().includes(this.searchProjectMemberText.toLowerCase())))
        && (!this.isShowDeactiveMember ? member.ptype !== APP_CONSTANT.EnumUserType.DeActive : true))
    );
  }

  selectTeam(user: UserDto, index) {
    this.displayActiveMembers.splice(index, 1);
    const member = {
      userId: user.id,
      isActive: user.isActive,
      jobTitle: user.jobTitle,
      level: user.level,
      name: user.name,
      type: user.type,
      avatarFullPath: user.avatarFullPath,
      ptype: this.projectMembers.length == 0 ? APP_CONSTANT.EnumUserType.PM : APP_CONSTANT.EnumUserType.Member,
      branch: user.branch,
      emailAddress: user.emailAddress,
      branchColor: user.branchColor,
      branchDisplayName: user.branchDisplayName,
      isTemp:false
    } as ProjectUserDto;

    this.projectMembers.push(member);
    this.displayProjectMembers.push(member)
    this.activeMembers.splice(this.activeMembers.findIndex(s => s.id === user.id), 1);
  }


  checkAllTask($e: MatCheckboxChange) {
    this.projectTasks.forEach(el => {
      el.billable = $e.source.checked
    })
    this.checkAllTaskStatus = $e.source.checked ? 1 : 0;
  }

  removeTask(ptask: ProjectTaskDto, index) {
    this.projectTasks.splice(index, 1);
    const task = {
      id: ptask.taskId,
      name: ptask.name,
      isDeleted: false,
      type: ptask.type,
    } as TaskDto;
    this.availableTask.push(task);
    this.updateCheckAllStatus();
  }

  selectTask(task: TaskDto, index) {

    this.availableTask.splice(index, 1);
    const ptask = {
      taskId: task.id,
      billable: true,
      name: task.name,
      type: task.type,
    } as ProjectTaskDto;
    this.projectTasks.push(ptask);
    this.updateCheckAllStatus();
  }

  // PROCESSING TARGET_USER

  isExistShadow() {
    return this.projectMembers && this.projectMembers.some(s => s.ptype == this.APP_CONSTANT.EnumUserType.Shadow);
  }


  save() {

    this.project.tasks = this.projectTasks.map((task) => ({
      taskId: task.taskId,
      billable: task.billable,
    } as TaskProjectDto));
    this.project.users = this.projectMembers.map((member) => ({
      userId: member.userId,
      type: member.ptype,
      isTemp: member.isTemp
    }));
    if (this.isExistShadow()) {
      this.project.projectTargetUsers = this.projectTargetUsers.map((targetUser) => ({
        userId: targetUser.userId,
        roleName: targetUser.roleName,
      }));
    } else this.project.projectTargetUsers = [];
    if (_.isEmpty(this.projectTasks)) {
      abp.message.error("Project must have at least one task!")
      return;
    }
    if (_.isEmpty(this.projectMembers)) {
      abp.message.error("Project must have at least one member!")
      return;
    }
    if (!this.projectMembers.some(x => x.ptype === APP_CONSTANT.EnumUserType.PM)) {
      abp.message.error("Project must have a PM!")
      return;
    }
    //this.project.status = APP_CONSTANT.EnumProjectStatus.Active;

    this.isSaving = true;
    this.projectManagerService.save(this.project).subscribe(res => {
      this.isSaving = false;
      if (res.success == true) {
        if (this.project.id == null) {
          this.notify.success(this.l('Create Project Successfully'));
        }
        else {
          this.notify.success(this.l('Update Project Successfully'));
        }
        this.respone = 1;
        this.close(this.respone);
      }
    }, err => {
      this.isSaving = false;
    })

  }

  close(res): void {
    this._dialogRef.close(res);
  }

  removeFromTargetUsers(member: DisplayProjectTargetUserDto, index) {
    var user = {
      avatarFullPath: member.avatarFullPath,
      id: member.userId,
      isActive: member.isActive,
      jobTitle: member.jobTitle,
      level: member.level,
      name: member.name,
      type: member.type,
      emailAddress: member.emailAddress
    } as UserDto;
    this.projectTargetUsers.splice(index, 1);
    this.availableTargetUsers.push(user);
    this.displayAvailableTargetUsers.push(user);
    // this.updateDisplayAvaibleTargetUsers();
  }

  searchTargetUser() {
      this.displayAvailableTargetUsers = this.availableTargetUsers.filter(
        targetUser => targetUser.name.search(new RegExp(this.searchTargetUserText, "ig")) > -1 
        || targetUser.emailAddress.toLowerCase().includes(this.searchTargetUserText.toLowerCase()));
  }

  selectTargetUser(mem: UserDto, index) {
    var ptuser = {
      userId: mem.id,
      avatarFullPath: mem.avatarFullPath,
      isActive: mem.isActive,
      jobTitle: mem.jobTitle,
      level: mem.level,
      name: mem.name,
      roleName: "",
      type: mem.type,
      emailAddress: mem.emailAddress
    } as DisplayProjectTargetUserDto
    this.projectTargetUsers.push(ptuser);    
      let i = this.availableTargetUsers.indexOf(mem);
    this.availableTargetUsers.splice(i, 1);;
    this.displayAvailableTargetUsers.splice(index, 1);

  }

  updateCheckAllStatus() {
    if (this.projectTasks.every(el => el.billable == false)) {
      this.checkAllTaskStatus = 0;
    }
    else if (this.projectTasks.every(el => el.billable == true)) {
      this.checkAllTaskStatus = 1;
    }
    else {
      this.checkAllTaskStatus = 2;
    }
  }

  getAllCustomer() {
    this.customerService.getAllCustomer().subscribe(res => {
      this.listCustomer = res.result;
      this.listCustomerFilter = this.listCustomer;
    })
  }

  filterCustomer(): void {
    if (this.customerSearch.value) {
      this.listCustomer = this.listCustomerFilter.filter(data => data.name.toLowerCase().includes(this.customerSearch.value.toLowerCase().trim()));
    } else {
      this.listCustomer = this.listCustomerFilter.slice();
    }
  }

  createCustomer(): void {
    let customer = {} as CustomerDto;
    this.showDialog(customer);
  }

  editCustomer(customer: CustomerDto): void {
    this.showDialog(customer);
  }


  showDialog(customer: CustomerDto): void {
    let item = { id: customer.id, name: customer.name, address: customer.address } as CustomerDto;
    const showCreateOrEditProjectDialog = this._dialog.open(CreateEditCustomerComponent, {
      data: item
    });

    showCreateOrEditProjectDialog.afterClosed().subscribe(result => {
      var customer = {
        id: result.id,
        name: result.name,
        code: result.code,
        address: result.address
      } as CustomerDto
      this.listCustomer.push(customer);
      this.listCustomerFilter.push(customer);
      this.project.customerId = result.id;
    });
  }
  checkNoTifyKomu(value: any): any {
    if (value.checked) {
      this.project.isNotifyToKomu = true;
    } else {
      this.project.isNotifyToKomu = false;
    }
  }
  searchMemberTarget() {
    if (this.statusForFilter === "") {
      this.displayAvailableTargetUsers = this.availableTargetUsers
      return
    }
    this.displayAvailableTargetUsers = this.availableTargetUsers.filter(
      member => (member.isActive === this.statusForFilter)
    );
  }
}