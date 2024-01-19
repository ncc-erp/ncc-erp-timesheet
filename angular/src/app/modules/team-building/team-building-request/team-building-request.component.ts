import { RequestDetailComponent } from './request-detail/request-detail.component';
import { Component, Injector, OnInit } from "@angular/core";
import { MatDialog } from "@angular/material";
import { DomSanitizer } from "@angular/platform-browser";
import { PERMISSIONS_CONSTANT } from "@app/constant/permission.constant";
import { TeamBuildingRequestService } from "@app/service/api/team-building-request.service";
import {
  PagedListingComponentBase,
  PagedRequestDto,
} from "@shared/paged-listing-component-base";
import * as _ from "lodash";
import { finalize } from "rxjs/operators";
import { DisburseRequestComponent } from "./disburse-request/disburse-request.component";
import { ActionRequestHistoryEnum, LIST_MONTHS, PagedRequestHistoryDto, RemainingMoneyEnum, RemainingMoneystatus, StatusTeamBuildingRequest, StatusTeamBuildingRequestEnum, TeamBuildingRequestDto } from '../const/const';
import { EditRequestComponent } from './edit-request/edit-request.component';

@Component({
  selector: "app-team-building-request",
  templateUrl: "./team-building-request.component.html",
  styleUrls: ["./team-building-request.component.css"],
})
export class TeamBuildingRequestComponent
  extends PagedListingComponentBase<TeamBuildingRequestDto>
  implements OnInit
{
  TeamBuilding_ViewDetailRequest = PERMISSIONS_CONSTANT.TeamBuilding_Request_ViewDetailRequest;
  TeamBuilding_DisburseRequest = PERMISSIONS_CONSTANT.TeamBuilding_Request_DisburseRequest;
  TeamBuilding_EditRequest = PERMISSIONS_CONSTANT.TeamBuilding_Request_EditRequest;
  TeamBuilding_ReOpenRequest = PERMISSIONS_CONSTANT.TeamBuilding_Request_ReOpenRequest;
  TeamBuilding_CancelRequest = PERMISSIONS_CONSTANT.TeamBuilding_Request_CancelRequest;
  TeamBuilding_RejectRequest = PERMISSIONS_CONSTANT.TeamBuilding_Request_RejectRequest;

  public searchText: string;
  public listYear = [];
  public year: number;
  public status: StatusTeamBuildingRequestEnum = StatusTeamBuildingRequestEnum.All;
  public listRequest: TeamBuildingRequestDto[] = [];
  public listStatus: StatusTeamBuildingRequest[] = [
    { value: StatusTeamBuildingRequestEnum.All, title: "All" },
    { value: StatusTeamBuildingRequestEnum.Pending, title: "Pending" },
    { value: StatusTeamBuildingRequestEnum.Done, title: "Done" },
    { value: StatusTeamBuildingRequestEnum.Rejected, title: "Rejected" },
    { value: StatusTeamBuildingRequestEnum.Cancelled, title: "Cancelled" },
  ];
  public listRemainingMoneyStatus: RemainingMoneystatus[] = [
    { value: RemainingMoneyEnum.Remaining, title: "Remaining" },
    { value: RemainingMoneyEnum.Done, title: "Done" },
  ];
  public inputRequest = {} as PagedRequestHistoryDto;
  public listMonth = LIST_MONTHS;
  public month: number = -1;

  constructor(
    injector: Injector,
    private _teamBuildingRequestService: TeamBuildingRequestService,
    public _dialog: MatDialog,
    public sanitizer: DomSanitizer
  ) {
    super(injector);
    var d = new Date().getFullYear();
    for (let i = d - 5; i <= d + 2; i++) {
      this.listYear.push(i);
    }
    this.year = d;
  }

  ngOnInit() {
    this.setDefaultFilter();
    this.refresh();
  }

  protected list(
    request: PagedRequestDto,
    pageNumber: number,
    finishedCallback: Function
  ): void {
    request.filterItems = [];
    this.inputRequest = {
      gridParam: request,
      year: this.year,
      month: this.month,
      status: this.status == -1 ? null : this.status,
    };

    if (this.searchText) {
      request.searchText = this.searchText;
    }
    this.subscriptions.push(
      this._teamBuildingRequestService
        .getAllRequest(this.inputRequest)
        .pipe(finalize(() => finishedCallback()))
        .subscribe((rs) => {
          this.listRequest = rs.result.items;
          this.showPaging(rs.result, pageNumber);
        })
    );
  }

  setDefaultFilter() {
    this.year = new Date().getFullYear();
    this.status = -1;
  }
  handleFilterYear(e) {
    this.year = e.value;
    this.refresh();
  }

  handleFilterMonth(m) {
    this.month = m.value;
    this.refresh();
  }

  handleFilterStatus(e) {
    this.status = e.value;
    this.refresh();
  }

  rejectRequest(id: number): void {
    abp.message.confirm(
      "Do you want to reject this record?",
      (result: boolean) => {
        if (result) {
          this.isTableLoading = true;
          this._teamBuildingRequestService.rejectRequest(id).subscribe(
            (res) => {
              this.notify.success(this.l("Rejected request Successfully"));
              this.isTableLoading = false;
              this.refresh();
            },
            () => (this.isTableLoading = false && this.refresh())
          );
        }
      }
    );
  }

  cancelRequest(id: number): void {
    abp.message.confirm(
      "Do you want to cancel this record?",
      (result: boolean) => {
        if (result) {
          this.isTableLoading = true;
          this._teamBuildingRequestService.cancelRequest(id).subscribe(
            (res) => {
              this.notify.success(this.l("Cancelled request Successfully"));
              this.isTableLoading = false;
              this.refresh();
            },
            () => (this.isTableLoading = false && this.refresh())
          );
        }
      }
    );
  }

  disburse(item): void {
    const dialogRef = this._dialog.open(DisburseRequestComponent, {
      disableClose: true,
      width: "1200px",
      height: "80vh",
      data: {
        id: item.id,
        requesterId: item.requesterId,
        requestMoney: item.requestMoney,
        invoiceAmount: item.invoiceAmount,
      },
    });
    dialogRef.afterClosed().subscribe((result) => {
      if (result) {
        abp.notify.success("Disburse request successful");
        this.refresh();
      }
    });
  }
  pmSendRequest(id: number): void {
    const dialogRef = this._dialog.open(RequestDetailComponent, {
      disableClose: true,
      width: "900px",
      data: id,
    });
    dialogRef.afterClosed().subscribe((result) => {
      if (result) {
        this.refresh();
      }
    });
  }

  pmEditRequest(id: number): void {
    const dialogRef = this._dialog.open(EditRequestComponent, {
      disableClose: true,
      width: window.innerWidth >= 1000 ? "1000px" : "90%",
      data: id,
    });
    dialogRef.afterClosed().subscribe((result) => {
      if (result) {
        this.refresh();
      }
    });
  }

  reOpenRequest(id: number): void {
    abp.message.confirm(
      "Do you want to Re-open this record?",
      (result: boolean) => {
        if (result) {
          this.isTableLoading = true;
          this._teamBuildingRequestService.reOpenRequest(id).subscribe(
            (res) => {
              this.notify.success(this.l("Re-opened request Successfully"));
              this.isTableLoading = false;
              this.refresh();
            },
            () => (this.isTableLoading = false && this.refresh())
          );
        }
      }
    );
  }

  public handleAction(type: ActionRequestHistoryEnum, item: TeamBuildingRequestDto) {
    if (type === ActionRequestHistoryEnum.Disburse) {
      this.disburse(item);
    } else if (type === ActionRequestHistoryEnum.Reject) {
      this.rejectRequest(item.id);
    } else if(type === ActionRequestHistoryEnum.Cancel) {
      this.cancelRequest(item.id);
    } else if(type === ActionRequestHistoryEnum.ViewDetail) {
      this.pmSendRequest(item.id);
    } else if(type === ActionRequestHistoryEnum.Edit) {
      this.pmEditRequest(item.id);
    } else if(type === ActionRequestHistoryEnum.ReOpen) {
      this.reOpenRequest(item.id);
    }
  }

  protected delete(entity: TeamBuildingRequestDto): void {
    throw new Error("Method not implemented.");
  }
}
