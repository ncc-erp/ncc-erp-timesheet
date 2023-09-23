import { DateFilterType } from "@shared/AppEnums";
import { FilterDto } from "@shared/paged-listing-component-base";

export class GetInternsInfoDto{
    basicTrainerIds : number[];
    branchIds : number[];
    startDate : any
    endDate : any
    dateFilterType : DateFilterType;
    skipCount: number;
    maxResultCount: number;
    searchText: string;
    filterItems: FilterDto[] = [];
    sort: string;
    sortDirection: number;
  }