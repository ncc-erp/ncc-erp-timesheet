import { PagedRequestDto } from "@shared/paged-listing-component-base";

export class GetAllBackgroundJobsDto{
    id: number;
    jobType: string;
    jobArgs: string;
    tryCount: number;
    lastTryTime: string;
    nextTryTime: string;
    isAbandoned: boolean;
    priority: number;
    creationTime: string;
    creatorUserId: number;
    description: string;
    subJobType: string;
}
export class InputToGetAllDto{
    searchById: string;
    param: PagedRequestDto;
  }