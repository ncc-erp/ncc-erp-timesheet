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
    if (!this.searchText || this.searchText.trim() === "") {
      this.filteredProjects = [...this.projects];
    } else {
      const search = this.searchText.toLowerCase().trim();
      this.filteredProjects = this.projects.filter(
        (item) =>
          item.userName.toLowerCase().includes(search) ||
          item.officeName.toLowerCase().includes(search)
      );
    }
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

  toggleSelectAll(): void {
    if (this.isAllSelected()) {
      this.branchIds = [];
    } else {
      this.branchIds = this.listBranch.map((branch) => branch.id);
    }
    this.searchOrFilter();
  }

  clearAllFilters(): void {
    this.searchText = "";
    this.branchSearchText = "";
    this.branchIds = [];
    this.limit = undefined;
    this.searchOrFilter();
  }

  refresh(): void {
    this.searchOrFilter();
  }
}
