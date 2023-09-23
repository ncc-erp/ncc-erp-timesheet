import { ActionDialog } from "@shared/AppEnums";
import { EnumSort } from "./retro-detail-dto";

export interface RetroDto {
  id: number;
  name: string;
  startDate: string;
  endDate: string;
  deadline: string;
  status: number;
}

export interface RetroActionDialogData {
  item: RetroDto;
  action: ActionDialog;
}

export enum EnumSortName {
  StartDate = "startDate",
}

export interface RetroSort {
  name: EnumSortName;
  value: EnumSort;
}
