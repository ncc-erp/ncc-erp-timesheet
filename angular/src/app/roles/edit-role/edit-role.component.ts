import { SelectionModel } from "@angular/cdk/collections";
import { NestedTreeControl } from "@angular/cdk/tree";
import {
  AfterViewChecked,
  ChangeDetectorRef,
  Component,
  Injector,
} from "@angular/core";
import { FormControl } from "@angular/forms";
import { MatTreeNestedDataSource } from "@angular/material/tree";
import { ActivatedRoute } from "@angular/router";
import { BranchService } from "@app/service/api/branch.service";
import { MemberService } from "@app/service/api/member.service";
import { UserDto } from "@app/service/api/model/project-Dto";
import { UserService } from "@app/service/api/user.service";
import { appModuleAnimation } from "@shared/animations/routerTransition";
import { AppComponentBase } from "@shared/app-component-base";
import { AppConsts } from "@shared/AppConsts";
import {
  BranchDto,
  Permission,
  PermissionDto,
  RoleDto,
  RolePermissionDto,
  RoleServiceProxy,
} from "@shared/service-proxies/service-proxies";
@Component({
  selector: "app-edit-role",
  animations: [appModuleAnimation()],
  templateUrl: "./edit-role.component.html",
  styleUrls: ["./edit-role.component.css"],
})
export class EditRoleComponent
  extends AppComponentBase
  implements AfterViewChecked
{
  selection = new SelectionModel<string>(true, []);
  treeControl: NestedTreeControl<any>;
  dataSource: MatTreeNestedDataSource<any>;
  roleId: number;
  role: RoleDto = new RoleDto();
  permissions: RolePermissionDto = new RolePermissionDto();
  grantedPermissionNames: string[];
  activeMembers: UserDto[] = [];
  displayActiveMembers: UserDto[] = [];
  selectedMembers: UserDto[] = [];
  searchMemberText = "";
  userTypes = [
    { value: 0, label: "Staff" },
    { value: 1, label: "Internship" },
    { value: 2, label: "Collaborator" },
  ];
  userTypeForFilter = -1;
  userBranchForFilter = 0;
  userLevelForFilter = -1;
  isStatic: boolean;
  path = "";
  title = "";

  listBranch: BranchDto[] = [];
  branchSearch: FormControl = new FormControl("");
  listBranchFilter: BranchDto[];

  constructor(
    private activatedRoute: ActivatedRoute,
    private _roleService: RoleServiceProxy,
    private ref: ChangeDetectorRef,
    private memberService: MemberService,
    private userService: UserService,
    private branchService: BranchService,
    injector: Injector
  ) {
    super(injector);
    this.dataSource = new MatTreeNestedDataSource<Permission>();
    this.treeControl = new NestedTreeControl<Permission>(
      (node) => node.childrens
    );
  }

  hasChild = (_: number, node: Permission) =>
    !!node.childrens && node.childrens.length > 0;

  ngOnInit() {
    this.activatedRoute.url.forEach((v) => (this.path = v[1].path));
    this.path === "view-detail"
      ? (this.title = "View Detail Role")
      : (this.title = "Edit Role");
    this.roleId = this.activatedRoute.snapshot.params.id;
    this._roleService.getRoleForEdit(this.roleId).subscribe((result: any) => {
      console.log(result);
      this.role = result.result.role;
      this.selectedMembers = result.result.users;
      this.isStatic = result.result.role.isStatic;
      this.grantedPermissionNames = result.result.grantedPermissionNames;
      this.dataSource.data = result.result.permissions;
      this.treeControl.dataNodes = result.result.permissions;
      this.treeControl.dataNodes.forEach((node) =>
        this.initSelectionList(node)
      );
      this.getAllMember();
    });
    this.getListBranch();
  }

  getListBranch() {
    this.branchService.getAllBranchFilter(true).subscribe((res) => {
      this.listBranch = res.result;
      this.listBranchFilter = this.listBranch;
    });
  }

  filterBranch(): void {
    if (this.branchSearch.value) {
      this.listBranch = this.listBranchFilter.filter((data) =>
        data.displayName
          .toLowerCase()
          .includes(this.branchSearch.value.toLowerCase().trim())
      );
    } else {
      this.listBranch = this.listBranchFilter.slice();
    }
  }

  getAllMember() {
    this.memberService.getUserNoPagging().subscribe((res) => {
      const allMembers = res.result as UserDto[];
      this.activeMembers = allMembers.filter(
        (mem) =>
          mem.isActive === true &&
          !this.selectedMembers.some((s) => mem.id === s.id)
      );
      this.displayActiveMembers = this.activeMembers.filter((x) => true);
    });
  }

  selectTeam(user: UserDto, index) {
    const u = this.displayActiveMembers.splice(index, 1);
    this.selectedMembers.push(u[0]);
    this.activeMembers.splice(index, 1);
    this.userService.changeUserRole(u[0].id, this.role.name).subscribe();
  }

  removeMemberFromProject(user: UserDto, index) {
    const u = this.selectedMembers.splice(index, 1);
    this.displayActiveMembers.push(u[0]);
    this.activeMembers.push(u[0]);
    this.userService.changeUserRole(u[0].id, this.role.name).subscribe();
  }

  searchMember() {
    this.displayActiveMembers = this.activeMembers.filter(
      (member) =>
        (!this.searchMemberText ||
          member.name.search(new RegExp(this.searchMemberText, "ig")) > -1) &&
        (this.userBranchForFilter === 0 ||
          member.branchId === this.userBranchForFilter) &&
        (this.userTypeForFilter < 0 ||
          member.type === this.userTypeForFilter) &&
        (this.userLevelForFilter < 0 ||
          member.level === this.userLevelForFilter)
    );
  }

  ngAfterViewChecked() {
    this.ref.detectChanges();
  }

  initSelectionList(node: Permission) {
    const selectedList = this.grantedPermissionNames as any;
    if (selectedList.includes(node.name)) {
      this.selected(node);
    }

    if (!node.childrens || node.childrens.length === 0) {
      return;
    } else {
      node.childrens.forEach((child) => this.initSelectionList(child));
    }
  }

  isSelected(node: Permission) {
    return this.selection.isSelected(node.name);
  }

  deselected(node: Permission) {
    this.selection.deselect(node.name);
  }
  selected(node: Permission) {
    this.selection.select(node.name);
  }

  descendantsAllSelected(node: Permission): boolean {
    const descendants = this.treeControl.getDescendants(node);
    const descAllSelected = descendants.every((child) =>
      this.isSelected(child)
    );
    descAllSelected ? this.selected(node) : this.deselected(node);
    return descAllSelected;
  }

  descendantsPartiallySelected(node: Permission): boolean {
    const descendants = this.treeControl.getDescendants(node);
    const result = descendants.some((child) => this.isSelected(child));
    return result && !this.descendantsAllSelected(node);
  }

  todoLeafItemSelectionToggle(node: Permission) {
    this.isSelected(node) ? this.deselected(node) : this.selected(node);
    this.descendantsPartiallySelected(node);
    this.onSaveData(node);
  }

  todoItemSelectionToggle(node: Permission) {
    this.isSelected(node) ? this.deselected(node) : this.selected(node);
    const descendants = this.treeControl.getDescendants(node);
    descendants.forEach((child) => {
      this.isSelected(node) ? this.selected(child) : this.deselected(child);
    });
    this.onSaveData(node);
  }

  // Trước khi gọi API
  // Cho những node có trạng thái indeterminate vào list.
  selectOrDeselectAllIndeterminateParents(doSelect: boolean) {
    this.treeControl.dataNodes.forEach((parent) => {
      this.selectOrDeselectTheIndeterminateParent(parent, doSelect);
    });
  }

  selectOrDeselectTheIndeterminateParent(node: Permission, doSelect: boolean) {
    if (!node.childrens || node.childrens.length < 1) {
      return;
    }
    const descendants = this.treeControl.getDescendants(node);
    const descSomeSelected = descendants.some((child) =>
      this.isSelected(child)
    );

    if (descSomeSelected) {
      if (doSelect) {
        this.selected(node);
      } else if (!this.descendantsAllSelected(node)) {
        this.deselected(node);
      }
    }

    descendants.forEach((child) =>
      this.selectOrDeselectTheIndeterminateParent(child, doSelect)
    );
  }

  onBack() {
    history.back();
  }

  onSave() {
    this._roleService.update(this.role).subscribe(() => {
      this.notify.success(this.l("Saved successfully"));
    });
  }

  onSaveData(node: Permission) {
    node.isLoading = true;
    const descendants = this.treeControl.getDescendants(node);
    if (descendants) {
      descendants.forEach((child) => (child.isLoading = true));
    }
    this.selectOrDeselectAllIndeterminateParents(true);
    this.permissions.permissions = this.selection.selected;
    this.permissions.id = this.role.id;
    this._roleService.changeRolePermission(this.permissions).subscribe(() => {
      node.isLoading = false;
      if (descendants) {
        descendants.forEach((child) => (child.isLoading = false));
      }
    });
  }
}
