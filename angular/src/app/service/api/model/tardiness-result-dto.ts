import { TardinessDto } from './report-timesheet-Dto';

export class TardinessResultDto {
  gridResult: {
    totalCount: number;
    items: TardinessDto[];
  };
  totalPunishmentAmount: number;
}
