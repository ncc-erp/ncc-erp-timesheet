import { PagedRequestDto } from "@shared/paged-listing-component-base";
export class CapabilitySettingDto {
  userType: number;
  userTypeName: string;
  positionId: number;
  positionName: string;
  capabilityId: number;
  capabilityName: string;
  coefficient: number;
  id: number;
  guildeLine: string;
  type: number;
}
export class GetPagingCapabilitySettingDto {
  userType: number;
  userTypeName: string;
  positionId: number;
  positionName: string;
  capabilities: CapabilityInCapabilitySettingDto[];
  guildeLine: string;
}
export class CapabilityInCapabilitySettingDto {
  capabilityId: number;
  capabilityName: string;
  coefficient: number;
  id: number;
}
export class CreateUpdateCapabilitySettingDto {
  id: number;
  userType: number;
  positionId: number;
  capabilityId: number;
  coefficient: number;
  guildeLine: string;
}
export class CloneCapabilitySettingDto {
  fromUserType: number;
  fromPositionId: number;
  toUserType: number;
  toPositionId: number;
}
export class ParamCapability {
  param: PagedRequestDto;
  type: number;
}
export class UserType {
  id: number;
  name: string;
}