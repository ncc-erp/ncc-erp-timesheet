import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { InputGetUserOtherProjectDto, SelectUserOtherProjectDto } from '@app/modules/team-building/const/const';
import { AppServiceBase } from '@app/service/api/appServiceBase.service';
import { RetroDetailDto } from '@app/service/api/model/retro-detail-dto';
import { RetroDetailService } from '@app/service/api/retro-detail.service';
import { BranchDto } from '@shared/service-proxies/service-proxies';
import * as _ from 'lodash';

@Component({
  selector: 'app-add-user-other-project-retro-result',
  templateUrl: './app-add-user-other-project-retro-result.component.html',
  styleUrls: ['./app-add-user-other-project-retro-result.component.css']
})
export class AppAddUserOtherProjectRetroResultComponent implements OnInit {

  @Input() retroResultAdding: RetroDetailDto[] = [];
  @Input() retroId: number;
  @Input() isCreateRequest: boolean;
  @Input() isShowBtnAddUser: boolean = true;

  @Output() onAddUsers: EventEmitter<SelectUserOtherProjectDto[]> = new EventEmitter<SelectUserOtherProjectDto[]>();
  @Output() onCancel: EventEmitter<boolean> = new EventEmitter<boolean>();

  public listUserNotInProject = [];
  public searchText: string = "";
  public branchIdUserOtherProject = "";
  public listBranchUserOtherProject: BranchDto[] = [];
  public branchSearchUserOtherProject: "";
  public listBranchFilterUserOtherProject: BranchDto[];
  public listSelectedItemUserOtherProject: SelectUserOtherProjectDto[] = [];
  public allCompleteUserOtherProject: boolean = false;
  public isLoading: boolean;

  constructor(
    public retroResultService: RetroDetailService,
    public appBaseService: AppServiceBase
  ) { }

  ngOnInit() {
    this.getListBranchUserOtherProject();
    this.getAllUser();
  }

  getAllUser() {
    this.isLoading = true;
    let listId = this.retroResultAdding.map((x) => x.userId);
    const input: InputGetUserOtherProjectRetroDto = {
      ids: listId,
      branchId: this.branchIdUserOtherProject,
      searchText: this.searchText,
      retroId: this.retroId
    };

    this.retroResultService.getAllUser(input).subscribe((data) => {
      this.listUserNotInProject = data.result;
      this.isLoading = false;
    });
  }

  handleCancel() {
    this.isShowBtnAddUser = !this.isShowBtnAddUser;
    this.onCancel.emit(this.isShowBtnAddUser);
  }

  getListBranchUserOtherProject() {
    this.appBaseService.getAllCurrentBranch().subscribe((res) => {
      this.listBranchUserOtherProject = this.listBranchFilterUserOtherProject = res.result;
    });
  }

  handleSearchBranchUserOtherProject() {
    const textSearch = this.branchSearchUserOtherProject.toLowerCase().trim();
    if (textSearch) {
      this.listBranchUserOtherProject = this.listBranchFilterUserOtherProject.filter((item) =>
        item.name.toLowerCase().trim().includes(textSearch)
      );
    } else {
      this.listBranchUserOtherProject = _.cloneDeep(this.listBranchFilterUserOtherProject);
    }
  }

  setAllUserOtherProject(completedUserOtherProject: boolean) {
    const self = this;
    this.allCompleteUserOtherProject = completedUserOtherProject;
    if (this.listUserNotInProject == null) {
      return;
    }
    this.listUserNotInProject.forEach((x) => {
      const item = self.listSelectedItemUserOtherProject.find(
        (item) => item.id == x.id
      );
      if (item) {
        item.selected = completedUserOtherProject;
      } else {
        self.listSelectedItemUserOtherProject.push({
          selected: completedUserOtherProject,
          id: x.id,
        });
      }
    });
  }

  someCompleteUserOtherProject(): boolean {
    if (this.listUserNotInProject == null || this.listUserNotInProject.length == 0) {
      return false;
    }

    return (
      this.listUserNotInProject.filter((t) => this.getIsSelectedUserOtherProject(t)).length > 0 &&
      !this.allCompleteUserOtherProject
    );
  }

  getIsSelectedUserOtherProject(item) {
    return this.listSelectedItemUserOtherProject.find((c) => c.id == item.id)
      ? this.listSelectedItemUserOtherProject.find((c) => c.id == item.id).selected
      : item.selected;
  }

  handleSelectRequestInfoItemUserOtherProject(index, $event) {
    this.listUserNotInProject[index].selected = $event.checked;
    const item = this.listSelectedItemUserOtherProject.find(
      (item) => item.id == this.listUserNotInProject[index].id
    );
    if (item) {
      item.selected = $event.checked;
    } else {
      this.listSelectedItemUserOtherProject.push({
        selected: $event.checked,
        id: this.listUserNotInProject[index].id,
      });
    }
    this.updateAllCompleteUserOtherProject();
  }

  updateAllCompleteUserOtherProject() {
    this.allCompleteUserOtherProject =
      this.listUserNotInProject != null &&
      this.listUserNotInProject.every((t) => this.getIsSelectedUserOtherProject(t));
  }

  handleAddUser() {
    this.onAddUsers.emit(this.listSelectedItemUserOtherProject.filter(item => item.selected));
    this.handleCancel();
  }
}

export class InputGetUserOtherProjectRetroDto {
  ids: number [] = [];
  retroId: number;
  searchText?: string;
  branchId?: string;
}
