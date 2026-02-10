import {
  Component,
  Injector,
  Input,
  OnInit,
  OnChanges,
  SimpleChanges,
  OnDestroy,
} from "@angular/core";
import { FormControl } from "@angular/forms";
import { DailyEmployeeReportService } from "@app/service/api/daily-employee-report.service";
import { OfficeWorkingItem } from "../Dto/branch-manage-dto";
import { BranchDto } from "@shared/service-proxies/service-proxies";
import { SortColumn, SortDirection, SelectAllText, SortArrow } from './enum/daily-employee-report.enum';
import { PagedListingComponentBase, PagedRequestDto } from "@shared/paged-listing-component-base";
import { finalize, debounceTime, distinctUntilChanged } from "rxjs/operators";
import { Subject } from "rxjs";

@Component({
  selector: "app-daily-employee-report",
  templateUrl: "./daily-employee-report.component.html",
  styleUrls: ["./daily-employee-report.component.css"],
})
export class DailyEmployeeReportComponent extends PagedListingComponentBase<OfficeWorkingItem> implements OnInit, OnChanges, OnDestroy {
  @Input() listBranch: BranchDto[];
  @Input() listBranchFilter: BranchDto[];

  branchIds: number[] = [];
  branchSearch = new FormControl();
  branchSearchText: string = "";
  searchText: string = "";
  limit: number;

  users: OfficeWorkingItem[] = [];
  isLoading: boolean = false;

  sortColumn: SortColumn = SortColumn.FullName;
  sortDirection: SortDirection = SortDirection.Asc;
  SortColumn = SortColumn;
  SortArrow = SortArrow;

  Math = Math;
  private searchSubject = new Subject<string>();

  constructor(
    private dailyEmployeeReportService: DailyEmployeeReportService,
    injector: Injector
  ) {
    super(injector);
  }

  ngOnInit(): void {
    if (!this.listBranchFilter) {
      this.listBranchFilter = this.listBranch || [];
    }

    this.subscriptions.push(
      this.searchSubject
        .pipe(debounceTime(500), distinctUntilChanged())
        .subscribe(() => {
          this.refresh();
        })
    );

    this.refresh();
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
    const branchCodes = this.getBranchCodes();
    this.dailyEmployeeReportService
      .getDailyProjectTimelogReport(
        request,
        branchCodes,
        this.sortColumn,
        this.sortDirection,
        this.limit
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
        },
      });
  }

  protected delete(entity: OfficeWorkingItem): void {
    throw new Error("Method not implemented.");
  }

  searchOrFilter(): void {
    this.refresh();
  }

  getBranchCodes(): number[] {
    if (
      !this.branchIds ||
      this.branchIds.length === 0
    ) {
      return this.listBranch ? this.listBranch.map((branch) => branch.id) : [];
    }
    return this.branchIds;
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
      this.sortDirection = this.sortDirection === SortDirection.Asc ? SortDirection.Desc : SortDirection.Asc;
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
    return this.sortDirection === SortDirection.Asc ? SortArrow.UP : SortArrow.DOWN;
  }

  onSearchChange(): void {
    this.searchSubject.next(this.searchText);
  }

  clearSearch(): void {
    this.searchText = "";
    this.searchSubject.next("");
  }

  onLimitEnter(): void {
    this.refresh();
  }

  toggleSelectAll(event?: MouseEvent): void {
    if (event) {
      event.stopPropagation();
    }

    if (this.isAllSelected() || (this.branchIds && this.branchIds.length > 0)) {
      this.branchIds = [];
    } else {
      this.branchIds = this.listBranch.map(b => b.id);
    }

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

  clearAllFilters(): void {
    this.searchText = "";
    this.branchSearchText = "";
    this.branchIds = [];
    this.limit = undefined;
    this.sortColumn = SortColumn.TotalAllLW;
    this.sortDirection = SortDirection.Desc;
    this.refresh();
  }

  get filteredBranches() {
    return this.listBranchFilter.filter(b => b.id && b.id !== 0);
  }

  round(value: number | string | null): string {
    if (value == null) return '0.0';
    const num = typeof value === 'string' ? parseFloat(value) : value;
    return isNaN(num) ? '0.0' : num.toFixed(1);
  }
}