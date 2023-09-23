import { MAT_DIALOG_DATA } from "@angular/material";
import { AppComponentBase } from "shared/app-component-base";
import { MatDialogRef } from "@angular/material";
import { TeamBuildingRequestService } from "@app/service/api/team-building-request.service";
import { Component, OnInit, Injector, Inject } from "@angular/core";
import {
  FileDto,
  RequestDto,
  ResponseDetailTeamBuildingHistoryDto,
} from "../../const/const";

@Component({
  selector: "app-request-detail",
  templateUrl: "./request-detail.component.html",
  styleUrls: ["./request-detail.component.css"],
})
export class RequestDetailComponent extends AppComponentBase implements OnInit {
  public requestInfo: RequestDto[] = [];
  public file: FileDto[] = [];
  public isLoading: boolean = false;
  public teamBuildingHistoryId: number = 0;
  public historyId: number = 0;
  public requestDetailInfo: ResponseDetailTeamBuildingHistoryDto = {
    teamBuildingDetailDtos: [],
    lastRemainMoney: 0,
    disburseMoney: 0,
    note: "",
  };
  constructor(
    injector: Injector,
    private _teamBuildingRequestService: TeamBuildingRequestService,
    public dialogRef: MatDialogRef<RequestDetailComponent>,
    @Inject(MAT_DIALOG_DATA) private data: any
  ) {
    super(injector);
  }

  ngOnInit() {
    this.teamBuildingHistoryId = this.data;
    this.historyId = this.data;
    this.getAllRequest();
    this.getAllRequestHistoryFile();
  }

  getAllRequest(): void {
    this.isLoading = true;
    this._teamBuildingRequestService
      .getDetailOfHistory(this.teamBuildingHistoryId)
      .subscribe(
        (res) => {
          this.requestDetailInfo = res.result;
          this.isLoading = false;
        },
        () => (this.isLoading = false)
      );
  }
  getAllRequestHistoryFile(): void {
    this.isLoading = true;
    this._teamBuildingRequestService
      .getAllRequestHistoryFileById(this.teamBuildingHistoryId)
      .subscribe(
        (res) => {
          this.file = res.result.map((fileData: any) => {
            return {
              id: fileData.id,
              fileName:
                fileData.fileName.slice(0, 10) +
                "..." +
                fileData.fileName.slice(fileData.fileName.lastIndexOf(".")),
              requestHistoryId: fileData.requestHistoryId,
              url: fileData.url,
            } as FileDto;
          });
          this.isLoading = false;
        },
        () => (this.isLoading = false)
      );
  }
  close(d) {
    this.dialogRef.close(d);
  }
  openUrl(url: string): void {
    window.open(url, "_blank");
  }
}
