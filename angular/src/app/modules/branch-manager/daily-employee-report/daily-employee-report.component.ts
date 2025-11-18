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
  DailyProjectTimelogReportResponse,
  OfficeWorkingItem,
} from "@app/service/api/daily-employee-report.service";
import { BranchDto } from "@shared/service-proxies/service-proxies";

type SortColumn = 'fullName' | 'branchName' | 'wfhLW' | 'officeLW' | 'totalAllLW' | 'wfhLM' | 'officeLM' | 'totalAllLM';
type SortDirection = 'asc' | 'desc' | '';

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

  // Sort state
  sortColumn: SortColumn | '' = '';
  sortDirection: SortDirection = '';

  reportData: DailyProjectTimelogReportResponse;
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

    console.log("=== Search/Filter ===");
    console.log("Branch Codes:", branchCodes);
    console.log("Limit:", this.limit);

    this.dailyEmployeeReportService
      .getDailyProjectTimelogReport(branchCodes, this.limit)
      .subscribe({
        next: (response) => {
          this.reportData = response;
          this.projects = response.officeWorkingData || [];
          this.applyFilters();

          console.log("Total Items:", this.projects.length);
          console.log("Filtered Items:", this.filteredProjects.length);

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
      this.branchIds.length === 0 ||
      this.branchIds.indexOf("all" as any) !== -1
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

      if (column === 'fullName' || column === 'branchName') {
        valueA = valueA.toLowerCase();
        valueB = valueB.toLowerCase();
        return direction === 'asc'
          ? valueA.localeCompare(valueB)
          : valueB.localeCompare(valueA);
      }

      const numA = Number(valueA) || 0;
      const numB = Number(valueB) || 0;
      return direction === 'asc' ? numA - numB : numB - numA;
    });
  }

  onSort(column: SortColumn): void {
    if (this.sortColumn === column) {
      if (this.sortDirection === 'asc') {
        this.sortDirection = 'desc';
      } else if (this.sortDirection === 'desc') {
        this.sortDirection = '';
        this.sortColumn = '';
      }
    } else {
      this.sortColumn = column;
      this.sortDirection = 'asc';
    }

    this.applyFilters();
  }

  getSortIcon(column: SortColumn): string {
    if (this.sortColumn !== column) return 'unfold_more';
    return this.sortDirection === 'asc' ? 'arrow_upward' : 'arrow_downward';
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
      this.branchIds.length === this.listBranch.length &&
      this.branchIds.indexOf('all' as any) === -1
    );
  }

  getSelectAllText(): string {
    if (this.isAllSelected()) {
      return 'Deselect All';
    } else if (this.branchIds && this.branchIds.length > 0) {
      return 'Deselect';
    } else {
      return 'Select All';
    }
  }

  clearAllFilters(): void {
    this.searchText = "";
    this.branchSearchText = "";
    this.branchIds = [];
    this.limit = undefined;
    this.sortColumn = '';
    this.sortDirection = '';
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