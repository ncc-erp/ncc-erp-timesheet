import {
  Component,
  Input,
  OnInit,
  OnChanges,
  SimpleChanges,
} from "@angular/core";
import { FormControl } from "@angular/forms";
import {
  DailyEmployeeReportService,
  OfficeWorkingItem,
} from "@app/service/api/daily-employee-report.service";
import { BranchDto } from "@shared/service-proxies/service-proxies";
import { SortColumn, SortDirection, SelectAllText, SortArrow } from './enum/daily-employee-report.enum';

@Component({
  selector: "app-daily-employee-report",
  templateUrl: "./daily-employee-report.component.html",
  styleUrls: ["./daily-employee-report.component.css"],
})
export class DailyEmployeeReportComponent implements OnInit, OnChanges {
  @Input() listBranch: BranchDto[];
  @Input() listBranchFilter: BranchDto[];

  branchIds: number[] = [];
  branchSearch = new FormControl();
  branchSearchText: string = "";
  searchText: string = "";
  limit: number;

  projects: OfficeWorkingItem[] = [];
  filteredProjects: OfficeWorkingItem[] = [];
  itemSize: number = 48;
  maxHeight = 400;

  sortColumn: SortColumn = SortColumn.None;
  sortDirection: SortDirection = SortDirection.None;

  reportData: OfficeWorkingItem[];
  isLoading: boolean = false;

  Math = Math;

  constructor(private dailyEmployeeReportService: DailyEmployeeReportService) {}

  ngOnInit(): void {
    if (!this.listBranchFilter) {
      this.listBranchFilter = this.listBranch || [];
    }
    this.searchOrFilter();
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes.listBranch && changes.listBranch.currentValue) {
      this.listBranchFilter = changes.listBranch.currentValue;
    }
  }

  searchOrFilter(): void {
    this.isLoading = true;
    const branchCodes = this.getBranchCodes();
    this.dailyEmployeeReportService
      .getDailyProjectTimelogReport(branchCodes, this.limit)
      .subscribe({
        next: (response) => {
          this.reportData = response;
          this.projects = this.reportData || [];
          this.applyFilters();
          this.isLoading = false;
        },
        error: (error) => {
          console.error("API Error:", error);
          this.isLoading = false;
        },
      });
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

  applyFilters(): void {
    let result = [...this.projects];

    if (this.searchText && this.searchText.trim() !== "") {
      const search = this.searchText.toLowerCase().trim();
      result = result.filter(
        (item) =>
          item.userName.toLowerCase().includes(search) ||
          item.branchName.toLowerCase().includes(search) ||
          item.fullName.toLowerCase().includes(search)
      );
    }

    if (this.sortColumn && this.sortDirection) {
      result = this.sortData(result, this.sortColumn, this.sortDirection);
    }

    this.filteredProjects = result;
  }

  sortData(data: OfficeWorkingItem[], column: SortColumn, direction: SortDirection): OfficeWorkingItem[] {
    if (!direction) return data;

    return [...data].sort((a, b) => {
      let valueA: any = a[column] || '';
      let valueB: any = b[column] || '';

      if (column === SortColumn.FullName || column === SortColumn.BranchName) {
        valueA = valueA.toLowerCase();
        valueB = valueB.toLowerCase();
        return direction === SortDirection.Asc
          ? valueA.localeCompare(valueB)
          : valueB.localeCompare(valueA);
      }

      const numA = Number(valueA) || 0;
      const numB = Number(valueB) || 0;
      return direction === SortDirection.Asc ? numA - numB : numB - numA;
    });
  }

  onSort(column: SortColumn): void {
    const currentCol = this.sortColumn as SortColumn;
    const currentDir = this.sortDirection as SortDirection;

    if (currentCol === column) {
      if (currentDir === SortDirection.Asc) {
        this.sortDirection = SortDirection.Desc;
      } else if (currentDir === SortDirection.Desc) {
        this.sortDirection = SortDirection.None;
        this.sortColumn = SortColumn.None;
      }
    } else {
      this.sortColumn = column as SortColumn;
      this.sortDirection = SortDirection.Asc;
    }

    this.applyFilters();
  }

  getSortIcon(column: SortColumn): string {
    if (this.sortColumn !== column) return SortArrow.NONE;
    return this.sortDirection === SortDirection.Asc ? SortArrow.UP : SortArrow.DOWN;
  }

  onSearchChange(): void {
    this.applyFilters();
  }

  clearSearch(): void {
    this.searchText = "";
    this.applyFilters();
  }

  onLimitEnter(): void {
    const hasValue = this.limit !== null && this.limit !== undefined && !isNaN(Number(this.limit));

    if (hasValue) {
      this.searchOrFilter();
    } else {
      this.limit = undefined;
      this.searchOrFilter();
    }
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

    this.searchOrFilter();
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
    this.sortColumn = SortColumn.None;
    this.sortDirection = SortDirection.None;
    this.searchOrFilter();
  }

  refresh(): void {
    this.searchOrFilter();
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