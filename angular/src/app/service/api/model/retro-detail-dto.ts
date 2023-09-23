import { ActionDialog } from "@shared/AppEnums";
import { FilterDto } from "@shared/paged-listing-component-base";

export interface RetroDetailDto {
  id: number;
  userId: number;
  userName: string;
  fullName: string;
  emailAddress: string;
  projectName: string;
  projectId: number;
  positionName: string;
  positionId: number;

  point: number;
  note: string;
  hideNote: boolean;

  branchId: number;
  userBranchId: number;
  branchColor: string;
  userBranchColor: string;
  branchName: string;
  userBranchName: string;

  type: number;
  userType: number;

  level: number;
  userLevel: number;

  retroName: string;

  updatedAt: string;
  updatedName: string;

  pmId: number;
  pmEmailAddress: string;
  pmFullName: string;
}

export interface RetroDetailCreateEditDto {
  id: number;
  userId: number;
  projectId: number;
  positionId: number;
  retroId: number;
  point: number;
  note: string;
  userName: string;
  branchId: number;
  userType: number;
  userLevel: number;
  projectName: string;
  pmId: number;
}

export interface RetroDetailDialogData {
  item: RetroDetailCreateEditDto;
  action: ActionDialog;
}

export interface ResponseFailDto {
  row: number;
  email: string;
  position: string;
  point: number;
  note: string;
  reasonFail: string;
}

export class ResultImportRetro {
  failedList: ResponseFailDto[];
  listEmailSuccess: RetroDetailCreateEditDto[];
  listWarning: string[]
  errorList?:string[]
}

export interface RetroDetailDialogData {
  item: RetroDetailCreateEditDto;
  action: ActionDialog;
}

export interface RetroDetailBranch {
  id: number;
  name: string;
}

export enum ActionEnum {
  Close = "close",
  Edit = "edit",
  Delete = "delete",
  Check = "check",
}
export interface Action {
  icon: ActionEnum;
  title: string;
}

export interface RetroDetailProject {
  name: string;
  code: string;
  id: number;
}

export interface RetroDetailMember {
  projectId: number;
  userId: number;
  fullNameAndEmail: string;
}

export enum EnumUserType {
  Staff = 0,
  Internship = 1,
  Collaborator = 2,
}

export enum EnumLevel {
  Intern_0 = 0,
  Intern_1 = 1,
  Intern_2 = 2,
  Intern_3 = 3,
  FresherMinus = 4,
  Fresher = 5,
  FresherPlus = 6,
  JuniorMinus = 7,
  Junior = 8,
  JuniorPlus = 9,
  MiddleMinus = 10,
  Middle = 11,
  MiddlePlus = 12,
  SeniorMinus = 13,
  Senior = 14,
  SeniorPlus = 15,
}

export interface RetroDetailFilter {
  id: EnumUserType | EnumLevel;
  name: string;
}

export interface RetroDetailBranch {
  id: number;
  name: string;
  displayName: string;
}

export enum EnumSort {
  ASC = 0,
  DEC = 1,
  NotArranged = 2,
}

export enum EnumSortName {
  FullName = "fullName",
  Point = "point",
}

export enum EnumStatus {
  Public = 0,
  Close = 1,
}

export interface RetroDetailSort {
  name: EnumSortName;
  value: EnumSort;
}

export class PagedRequestRetroDetailDto {
  gridParam: {
    skipCount: number;
    maxResultCount: number;
    searchText: string;
    filterItems: FilterDto[];
    sort: string;
    sortDirection: number;
  };
  usertypes?: number[];
  projecIds?: number[];
  userlevels?: number[];
  branchIds?: number[];
  positionIds?: number[];
  leftPoint?: number;
  rightPoint?: number;
}

export interface RetroDetailPm {
  projectId: number;
  pmId: number;
  pmFullName: string;
  pmEmailAddress: string;
  isDefault: boolean;
}
