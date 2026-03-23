import {
  Component,
  Injector,
  Input,
  OnInit,
  OnChanges,
  SimpleChanges,
  OnDestroy,
  ViewChild,
  AfterViewInit
} from "@angular/core";
import { DailyProjectTimelogReportService } from "@app/service/api/daily-project-report.service";
import { BranchDto } from "@shared/service-proxies/service-proxies";
import { SortColumn, SortDirection, SelectAllText, SortArrow } from './enum/daily-project-report.enum';
import { PagedListingComponentBase, PagedRequestDto } from "@shared/paged-listing-component-base";
import { finalize, debounceTime, distinctUntilChanged } from "rxjs/operators";
import { TotalTimelogProjectDto } from "../Dto/branch-manage-dto";
import { Subject } from "rxjs";

@Component({
  selector: "app-daily-project-report",
  templateUrl: "./daily-project-report.component.html",
  styleUrls: ["./daily-project-report.component.css"],
})
export class DailyProjectReportComponent
  extends PagedListingComponentBase<TotalTimelogProjectDto>
  implements OnInit, OnChanges, OnDestroy, AfterViewInit
{
  @Input() listBranch: BranchDto[];
  @Input() listBranchFilter: BranchDto[];

  branchIds: number[] = [];
  branchSearchText: string = "";
  minHours: number;

  users: TotalTimelogProjectDto[] = [];
  
  sortColumn: SortColumn | string = "";
  sortDirection: SortDirection | undefined = undefined;
  SortColumn = SortColumn;
  SortArrow = SortArrow;

  Math = Math;
  private searchSubject = new Subject<string>();

  @ViewChild("pagination") paginationControl: any;

  constructor(
    private dailyProjectReportService: DailyProjectTimelogReportService,
    injector: Injector
  ) {
    super(injector);
  }

  ngOnInit(): void {
    if (!this.listBranchFilter) {
      this.listBranchFilter = this.listBranch || [];
    }

    this.branchIds = this.appSession.user.branchId ? [this.appSession.user.branchId] : [];
    this.pageSize = 100;

    this.subscriptions.push(
      this.searchSubject
        .pipe(debounceTime(500), distinctUntilChanged())
        .subscribe(() => {
          this.refresh();
        })
    );

    this.refresh();
  }

  ngAfterViewInit(): void {
    if (this.paginationControl) {
      setTimeout(() => {
        this.paginationControl.selection = 100;
      });
    }
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes.listBranch && changes.listBranch.currentValue) {
      this.listBranchFilter = changes.listBranch.currentValue;
    }
  }

  ngOnDestroy(): void {
    super.ngOnDestroy();
  }

  protected list(
    request: PagedRequestDto,
    pageNumber: number,
    finishedCallback: Function
  ): void {
    const selectedBranchIds = this.branchIds || [];
    const isAllBranch = selectedBranchIds.length === 0 || (this.listBranch && selectedBranchIds.length === this.listBranch.length);
    const branchIdsToSend = isAllBranch ? [] : selectedBranchIds;

    this.dailyProjectReportService
      .getDailyProjectTimelogReport(
        request,
        branchIdsToSend,
        this.minHours,
        undefined,
        isAllBranch,
        this.sortColumn,
        this.sortDirection
      )
      .pipe(finalize(() => finishedCallback()))
      .subscribe({
        next: (response) => {
          const rs = response.result;
          this.users = rs.items || [];
          this.showPaging(rs, pageNumber);
        },
        error: (error) => {
          console.error("API Error:", error);
          this.users = [];
        },
      });
  }

  protected delete(entity: TotalTimelogProjectDto): void {
    throw new Error("Method not implemented.");
  }

  searchOrFilter(): void {
    this.refresh();
  }

  filterBranch(): void {
    if (!this.branchSearchText) {
      this.listBranchFilter = this.listBranch;
    } else {
      const searchText = this.branchSearchText.toLowerCase();
      this.listBranchFilter = this.listBranch.filter(
        (branch) => branch.name.toLowerCase().indexOf(searchText) > -1
      );
    }
  }

  onSort(column: SortColumn): void {
    if (this.sortColumn === column) {
      this.sortDirection =
        this.sortDirection === SortDirection.Asc
          ? SortDirection.Desc
          : SortDirection.Asc;
    } else {
      this.sortColumn = column;
      this.sortDirection = SortDirection.Desc;
    }
    this.refresh();
  }

  getSortIcon(column: SortColumn): string {
    if (this.sortColumn !== column) {
      return SortArrow.NONE;
    }
    return this.sortDirection === SortDirection.Asc
      ? SortArrow.UP
      : SortArrow.DOWN;
  }

  onSearchChange(): void {
    this.searchSubject.next(this.searchText);
  }

  clearSearch(): void {
    this.searchText = "";
    this.searchSubject.next("");
  }

  onMinHoursEnter(): void {
    this.refresh();
  }

  isAllSelected(): boolean {
    return (
      this.branchIds &&
      this.listBranch &&
      this.branchIds.length === this.listBranch.length
    );
  }

  getSelectAllText(): string {
    if (this.isAllSelected()) {
      return SelectAllText.DESELECT_ALL;
    } else if (this.branchIds && this.branchIds.length > 0) {
      return SelectAllText.DESELECT;
    } else {
      return SelectAllText.SELECT_ALL;
    }
  }
  toggleSelectAll(event?: MouseEvent): void {
    if (event) {
      event.stopPropagation();
    }

    if (this.isAllSelected() || (this.branchIds && this.branchIds.length > 0)) {
      this.branchIds = [];
    } else {
      this.branchIds = this.listBranch.map((b) => b.id);
    }

    this.refresh();
  }

  get filteredBranches() {
    return (this.listBranchFilter || []).filter((b) => b.id && b.id !== 0);
  }

  round(value: number | string | null): string {
    if (value == null) return '0.0';
    const num = typeof value === 'string' ? parseFloat(value) : value;
    return isNaN(num) ? '0.0' : num.toFixed(1);
  }
}