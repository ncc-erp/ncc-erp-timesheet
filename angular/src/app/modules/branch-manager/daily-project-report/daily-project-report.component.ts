import {
  Component,
  Input,
  OnInit,
  OnChanges,
  SimpleChanges,
} from "@angular/core";
import {
  DailyProjectTimelogReportService,
  DailyProjectTimelogReportResponse,
} from "@app/service/api/daily-project-report.service";
import { BranchDto } from "@shared/service-proxies/service-proxies";

interface ProjectReportItem {
  name: string;
  members: string[];
  totalTimelogLW: number;
  totalTimelogLM: number;
  memberCount: number;
}

type SortColumn = 'name' | 'memberCount' | 'totalTimelogLW' | 'totalTimelogLM';
type SortDirection = 'asc' | 'desc' | '';

@Component({
  selector: "app-daily-project-report",
  templateUrl: "./daily-project-report.component.html",
  styleUrls: ["./daily-project-report.component.css"],
})
export class DailyProjectReportComponent implements OnInit, OnChanges {
  @Input() listBranch: BranchDto[];
  @Input() listBranchFilter: BranchDto[];

  branchIds: number[] = [];
  branchSearchText: string = "";
  searchText: string = "";
  minHours: number;
  topN: number;

  projects: ProjectReportItem[] = [];
  filteredProjects: ProjectReportItem[] = [];
  itemSize: number = 48;
  maxHeight = 400;

  sortColumn: SortColumn | '' = '';
  sortDirection: SortDirection = '';

  reportData: DailyProjectTimelogReportResponse;
  isLoading: boolean = false;

  Math = Math;

  constructor(
    private dailyProjectReportService: DailyProjectTimelogReportService
  ) {}

  ngOnInit(): void {
    if (!this.listBranchFilter) {
      this.listBranchFilter = this.listBranch || [];
    }
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes.listBranch && changes.listBranch.currentValue) {
      this.listBranchFilter = changes.listBranch.currentValue;
    }
  }

  searchOrFilter(): void {
    if (!this.branchIds || this.branchIds.length === 0) {
      console.warn("No branches selected. Please select at least one branch.");
      this.projects = [];
      this.filteredProjects = [];
      this.isLoading = false;
      return;
    }

    this.isLoading = true;

    console.log("=== Search/Filter ===");
    console.log("Branch IDs selected:", this.branchIds);
    console.log("Min Hours:", this.minHours);
    console.log("Top N:", this.topN);

    this.dailyProjectReportService
      .getDailyProjectTimelogReport(
        this.branchIds,
        this.minHours,
        this.topN
      )
      .subscribe({
        next: (response) => {
          this.reportData = response;
          this.projects = (response.projects || []).map((p) => ({
            ...p,
            memberCount: p.members ? p.members.length : 0,
          }));
          this.applyFilters();

          console.log("Total Items:", this.projects.length);
          console.log("Filtered Items:", this.filteredProjects.length);

          this.isLoading = false;
        },
        error: (error) => {
          console.error("API Error:", error);
          this.projects = [];
          this.filteredProjects = [];
          this.isLoading = false;
        },
      });
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
          item.name.toLowerCase().includes(search) ||
          item.members.some((m) => m.toLowerCase().includes(search))
      );
    }

    if (this.sortColumn && this.sortDirection) {
      result = this.sortData(result, this.sortColumn, this.sortDirection);
    }

    this.filteredProjects = result;
  }

  sortData(
    data: ProjectReportItem[],
    column: SortColumn,
    direction: SortDirection
  ): ProjectReportItem[] {
    if (!direction) return data;

    return [...data].sort((a, b) => {
      let valueA: any = a[column] || 0;
      let valueB: any = b[column] || 0;

      if (column === 'name') {
        valueA = valueA.toLowerCase();
        valueB = valueB.toLowerCase();
        return direction === 'asc'
          ? valueA.localeCompare(valueB)
          : valueB.localeCompare(valueA);
      }

      return direction === 'asc' ? valueA - valueB : valueB - valueA;
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

isAllSelected(): boolean {
  return (
    this.branchIds &&
    this.listBranch &&
    this.branchIds.length === this.listBranch.length
  );
}

toggleSelectAll(event?: MouseEvent): void {
  if (event) {
    event.stopPropagation();
  }

  if (this.isAllSelected()) {
    this.branchIds = [];
    this.projects = [];
    this.filteredProjects = [];
  } else {
    this.branchIds = this.listBranch.map(b => b.id);
    this.searchOrFilter();
  }
}

onBranchSelectionChange(selectedIds: number[]): void {
  this.branchIds = selectedIds || [];
  if (this.branchIds.length > 0) {
    this.searchOrFilter();
  } else {
    this.projects = [];
    this.filteredProjects = [];
  }
}

  clearAllFilters(): void {
    this.searchText = "";
    this.branchSearchText = "";
    this.branchIds = [];
    this.minHours = undefined;
    this.topN = undefined;
    this.sortColumn = '';
    this.sortDirection = '';
    
    this.projects = [];
    this.filteredProjects = [];
  }

  refresh(): void {
    if (this.branchIds && this.branchIds.length > 0) {
      this.searchOrFilter();
    }
  }

  getMembersDisplay(members: string[]): string {
    if (!members || members.length === 0) return 'N/A';
    if (members.length <= 3) return members.join(', ');
    return `${members.slice(0, 3).join(', ')} +${members.length - 3} more`;
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