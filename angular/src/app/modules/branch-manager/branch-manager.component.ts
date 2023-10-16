import { Component, OnInit } from '@angular/core';
import { BranchService } from '@app/service/api/branch.service';
import { PositionDto } from '@app/service/api/model/position-dto';
import { PositionService } from '@app/service/api/position.service';
import { BranchDto } from '@shared/service-proxies/service-proxies';

@Component({
  selector: 'app-branch-manager',
  templateUrl: './branch-manager.component.html',
  styleUrls: ['./branch-manager.component.css']
})
export class BranchManagerComponent implements OnInit {
  public position: PositionDto[] = [];
  public positionFilter: PositionDto[];

  public branch: BranchDto[] = [];
  public branchFilter : BranchDto[];
  constructor(
    private positionService: PositionService,
    private branchService: BranchService,
  ) { }

  ngOnInit() {
    this.getListPosition();
    this.getListBranch();
  }

  getListPosition() {
    this.positionService.getAll().subscribe(res => {
      this.position = res.result;
      this.positionFilter = this.position;
    });
  }

  getListBranch() {
    this.branchService.getAllBranchFilter(true).subscribe(res => {
      this.branch = res.result;
      this.branchFilter = this.branch;
    });
  }

}
