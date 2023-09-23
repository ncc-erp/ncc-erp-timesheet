import { ActionDialog } from "@shared/AppEnums";

export class PositionDto {
    id: number;
    name: string;
    shortName: string;
    color: string;
    code: string;
}
export class PositionCreateEditDto extends PositionDto{
  
}
export interface PositionDialogData{
    item: PositionCreateEditDto;
    action: ActionDialog;
  }
  