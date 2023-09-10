import { ActionDialog } from "@shared/AppEnums";
import { PagedRequestDto } from "@shared/paged-listing-component-base";
import { Interface } from "readline";

export class TeamBuildingDetailDto {
    projectId: number;
    projectName: string;
    projectCode: string;
    pmEmailAddress: string;
    employeeFullName: string;
    employeeEmailAddress: string;
    employeeId: number;
    requesterFullName: string;
    requesterEmailAddress: string;
    requesterId: number;
    money: number;
    updatedAt: string;
    updatedName: string;
    createdUserName: string;
    lastModifierUserName: string;
    creationTime: string;
    lastModifierTime: string;
    status: StatusTeamBuildingDetailEnum;
    id: number;
    applyMonth: string;
  }

  export class PagedDetailDto {
    gridParam: PagedRequestDto;
    year: number;
    month: number;
    status?: StatusTeamBuildingDetailEnum;
  }

  export const DEFAULT_FILTER_VALUE = -1;

  export class StatusTeamBuildingDetail {
    title: string;
    value: StatusTeamBuildingDetailEnum;
  }

  export enum StatusTeamBuildingDetailEnum {
    All = -1,
    Open = 0,
    Requested = 1,
    Done = 2,
  }
  export class CreateEditTeambuildingDetailDto {
    id: number;
    projectId: number;
    employeeId: number;
    money: number;
  }
  export interface TeambuildingDetailDialogData {
    item: CreateEditTeambuildingDetailDto;
    action: ActionDialog;
  }

  export interface InputForGenerateDto {
    year?: number;
    month?: number;
  }

  export class PMRequestDto {
    titleRequest: string;
    listDetailId: number[];
    // listUrl: string[];
    // listFile: File[];
    note: string;
    invoiceAmount: string;
    listInvoiceRequestDto: InvoiceRequestDto[];
  }

  export class InvoiceRequestDto {
    invoiceUrl : string;
    amount : number;
    hasVat : boolean;
    invoiceImageName : string;
  }

  export class TeamBuildingDetailPMDto {
    projectId: number;
    projectName: string;
    projectCode: string;
    pmEmailAddress: string;
    employeeFullName: string;
    employeeEmailAddress: string;
    employeeId: number;
    requesterFullName: string;
    requesterEmailAddress: string;
    requesterId: number;
    money: number;
    updatedAt: string;
    updatedName: string;
    createdUserName: string;
    lastModifierUserName: string;
    creationTime: string;
    lastModifierTime: string;
    status: StatusTeamBuildingDetailEnum;
    id: number;
    applyMonth: number;
  }

  export class RequestDto {
    id: number;
    employeeId: number;
    employeeUserName: string;
    employeeEmailAddress: string;
    projectId: number;
    projectName: string;
    creationTime: string;
    money: number;
    selected?: boolean;
    applyMonth: string;
    status: number;
    requesterEmailAddress: string;
    branchId: number;
    branchName: string;
    branchColor: string;
    isNotInProject?: boolean = false;
    isWarning: boolean;
  }

  export class ProjectTeamBuildingDto {
    id: number;
    name: string;
    projectType: string;
    pmEmail: string;
    isAllowTeamBuilding: boolean;
  }

  export class ProjectType {
    title: string;
    value: ProjectTypeEnum;
  }

  export enum ProjectTypeEnum {
    All = -1,
    TimeAndMaterials = 0,
    FixedFee = 1,
    NoneBillable = 2,
    ODC = 3,
    Product = 4,
    Training = 5,
    NoSalary = 6,
  }

  export class DisburseTeamBuildingRequestDto {
    requestId: number;
    disburseMoney: number;
    finalMoney: number;
  }

  export interface branch {
    id: number;
    name: string;
  }

  export class TeamBuildingRequestDto {
    id: number;
    requesterId: number;
    fullNameRequester: string;
    emailRequester: string;
    titleRequest: string;
    requestMoney: number;
    disbursedMoney: number;
    remainingMoney: number;
    remainingMoneyStatus: RemainingMoneyEnum;
    creationTime: string;
    status: StatusTeamBuildingRequestEnum;
    invoiceAmount: number;
  }

  export class PagedRequestHistoryDto {
    gridParam: PagedRequestDto;
    year: number;
    month: number;
    status?: StatusTeamBuildingRequestEnum;
  }

  export class StatusTeamBuildingRequest {
    title: string;
    value: StatusTeamBuildingRequestEnum;
  }

  export class RemainingMoneystatus {
    title: string;
    value: RemainingMoneyEnum;
  }

  export enum StatusTeamBuildingRequestEnum {
    All = -1,
    Pending = 0,
    Done = 1,
    Rejected = 2,
    Cancelled = 3,
  }

  export enum RemainingMoneyEnum {
    Remaining = 0,
    Done = 1,
  }

  // dateInput: 'MM/YYYY'
  export const DATE_FORMATS = {
    parse: {
      dateInput: 'MM/YYYY'
    },
    display: {
      dateInput: 'MM/YYYY',
      monthYearLabel: 'MMM YYYY',
      dateA11yLabel: 'LL',
      monthYearA11yLabel: 'MMMM YYYY'
    }
  };

  export class InputToAddEmployee{
  employeeId: number;
  money: number;
  year?: number;
  month?: number;
}
export class GetProjectDto{
  id: number;
  name: string;
}
export class GetProjectUserDto{
  id: number;
  fullName: string;
}

export class FileDto {
  fileName: string;
  url: string;
  requestHistoryId: number;
  id: number;
}

export enum ActionRequestHistoryEnum {
  Disburse = "disburse",
  Reject = "reject",
  Cancel = "cancel",
  ViewDetail = "view detail",
  Edit = "edit",
  ReOpen = "re-open",
}

export class SelectProjectIsAllowTeamBuildingDto {
  projectId: number;
  isAllowTeamBuilding: boolean;
}

export class SelectTeamBuildingDetailDto {
  id: number;
  selected?: boolean;
  money: number;
  status: number;
}

 export const LIST_MONTHS = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12];

export class EditMoneyTeamBuildingDetailDto {
  id: number;
  money: number;
}

export interface EditMoneyTeamBuildingDetailDialogData{
  item: EditMoneyTeamBuildingDetailDto;
  action: ActionDialog;
}

export class InputGetAllDetailByRequestIdDto {
  teamBuildingHistoryId: number;
  projectId: number;
  branchId: number;
  month: number;
}

export class EditRequestDto {
  id: number;
  listDetailId: number[];
}

export interface ResponseDetailTeamBuildingHistoryDto {
  teamBuildingDetailDtos: RequestDto[];
  lastRemainMoney: number;
  note: string;
  disburseMoney: number;
}

export class InputGetUserOtherProjectDto {
  ids? : number[] = [];
  branchId?: string;
  searchText?: string;
}

export class SelectUserOtherProjectDto {
  id: number;
  selected?: boolean;
}

export class DisburseTeamBuildingRequestInfoDto {
  requestId : number;
  requestMoney : number;
  requesterEmail : number;
  requesterName : number;
  invoiceRequests : InvoiceRequestOfDisburseTeamBuidingRequestDto[];
}

export class InvoiceRequestOfDisburseTeamBuidingRequestDto {
  invoiceId : number;
  invoiceMoney : number;
  invoiceResourceName : string;
  invoiceResourceUrl : string;
  hasVAT : boolean;
}

export class DisburseDto {
  requestId : number;
  requesterId : number;
  disburseMoney : number;
  invoiceDisburseList : InvoiceDisburseDto[];
}

export class InvoiceDisburseDto {
  constructor (invoiceId : number, hasVAT : boolean) {
    this.invoiceId = invoiceId;
    this.hasVAT = hasVAT;
  }
  invoiceId : number;
  hasVAT : boolean;
}




