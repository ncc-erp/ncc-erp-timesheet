import { CapabilityType } from "@shared/AppEnums";
export interface CapabilityDto {
    id: number;
    name: string;
    type: CapabilityType;
    note: string;
    applySetting: any[];
    expanded: boolean;
  }
export interface CreateCapabilityDto {
    id?: number;
    name: string,
    type: CapabilityType,
    note: string
  }