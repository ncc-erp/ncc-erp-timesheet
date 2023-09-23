import { Component, Inject, Injector, OnInit } from "@angular/core";
import { FormControl } from "@angular/forms";
import {
  MatDialog,
  MatDialogRef,
  MatSelectChange,
  MAT_DIALOG_DATA,
} from "@angular/material";
import {
  ResultImportRetro,
  RetroDetailPm,
  RetroDetailProject,
} from "@app/service/api/model/retro-detail-dto";
import { RetroDetailService } from "@app/service/api/retro-detail.service";
import { AppComponentBase } from "@shared/app-component-base";
import { RetroDetailComponent } from "../retro-detail.component";

@Component({
  selector: "app-import-retro-detail",
  templateUrl: "./import-retro-detail.component.html",
  styleUrls: ["./import-retro-detail.component.css"],
})
export class ImportRetroDetailComponent
  extends AppComponentBase
  implements OnInit
{
  public selectedFiles: FileList;
  public retroId: number;
  public results = {} as ResultImportRetro;

  public isSaving: boolean = false;

  public selectedProjectName: string = "";
  projectFilter = [];
  projectId: number;
  projectSearch: FormControl = new FormControl("");
  projects = [];
  public listPm: RetroDetailPm[] = [];
  public listPmBySearch: RetroDetailPm[] = [];
  public listProject: RetroDetailProject[] = [];
  public listProjectBySearch: RetroDetailProject[] = [];
  public searchText = "";
  public searchPm = "";
  public pmId: number;

  constructor(
    injector: Injector,
    @Inject(MAT_DIALOG_DATA) public data: any,
    private retroDetailService: RetroDetailService,
    private dialog: MatDialog,
    private _dialogRef: MatDialogRef<RetroDetailComponent>
  ) {
    super(injector);
    this.projectSearch.valueChanges.subscribe(() => {
      this.filterProject();
    });
  }

  ngOnInit(): void {
    this.getProjects();
    this.retroId = this.data.retroId;
  }

  getProjects() {
    this.retroDetailService.getAllProject().subscribe((data) => {
      this.listProject = data.result;
      this.listProjectBySearch = data.result;
    });
  }

  handleOpenSelectProject() {
    if (this.searchText && this.listProjectBySearch.length === 0) {
      this.searchText = "";
      this.listProjectBySearch = this.listProject;
    }
  }

  handleChangeSearchText(e: Event) {
    this.searchText = (e.target as HTMLInputElement).value;
    this.listProjectBySearch = this.listProject;
    if (this.searchText) {
      this.listProjectBySearch = this.listProjectBySearch.filter((item) =>
        item.name.toLowerCase().includes(this.searchText.toLowerCase().trim())
      );
    }
  }

  projectChange(project: MatSelectChange) {
    this.projectId = project.value;
    this.getAllPm(project.value);
  }

  getAllPm(projectId?: number) {
    this.retroDetailService
      .getAllPms(this.retroId, projectId)
      .subscribe((value) => {
        this.listPm = value.result;
        this.pmId = this.listPm.filter(x=>{
          if(x.isDefault){
            return x
          }
        })[0].pmId;

        this.listPmBySearch = value.result;
      });
  }
  handleSearchPm(e: Event) {
    this.searchPm = (e.target as HTMLInputElement).value;
    this.listPmBySearch = this.listPm;
    if (this.searchPm) {
      this.listPmBySearch = this.listPm.filter(
        (item) =>
          item.pmEmailAddress
            .toLowerCase()
            .includes(this.searchPm.toLowerCase().trim()) ||
          item.pmFullName
            .toLowerCase()
            .includes(this.searchPm.toLowerCase().trim())
      );
    }
  }
  handleOpenSelectPm() {
    if (this.searchPm && this.listPmBySearch.length === 0) {
      this.searchPm = "";
      this.listPmBySearch = this.listPm;
    }
  }
  pmChange(pm: MatSelectChange) {
    this.pmId = pm.value;
  }
  filterProject(): void {
    if (this.projectSearch.value) {
      const temp: string = this.projectSearch.value.toLowerCase().trim();
      this.projects = this.projectFilter.filter((data) =>
        data.name.toLowerCase().includes(temp)
      );
    } else {
      this.projects = this.projectFilter.slice();
    }
  }

  public onSelectFile(event) {
    this.selectedFiles = event.target.files;
  }
  public onImportExcel() {
    if (!this.selectedFiles) {
      abp.message.error("Choose a file!");
      return;
    }
    const formData = new FormData();
    this.isSaving = true;
    formData.append("retroId", this.retroId.toString());
    formData.append("projectId", this.projectId.toString());
    formData.append("File", this.selectedFiles.item(0));
    formData.append("pmId", this.pmId.toString());

    let message: string = "";

    this.retroDetailService.importUser(formData).subscribe(
      (res) => {
        if (res.success) {
          this.isSaving = false;
          this.results = res.result;
          if (
            this.results.errorList != null &&
            this.results.errorList.length > 0
          ) {
            message = `<div class="retro-detail-import-message-result">${this.results.errorList.join(
              "<br/>"
            )}</div>`;
            abp.message.error(message, "Error", true);
            this.close(true);
          } else if (
            this.results.failedList != null &&
            this.results.failedList.length > 0
          ) {
            message = `<div class="retro-detail-import-message-result"><strong>Các user sau đã có trong retro ở project: ${
              this.selectedProjectName
            }</strong><br/>
            ${this.results.failedList.join("<br/>")}</div>`;
            abp.message.error(message, "Error", true);
            this.close(true);
          } else if (
            this.results.listWarning != null &&
            this.results.listWarning.length > 0
          ) {
            message = `<div class="retro-detail-import-message-result"><strong>Các user sau đã có trong đợt retro này: </strong> <br/>
            ${this.results.listWarning.join("<br/>")}
            </div><br/><strong>Bạn có chắc muốn import không?</strong>`;

            abp.message.confirm(
              message,
              "Warning",
              (rs) => {
                if (rs) {
                  let successMessage: string = "";
                  this.retroDetailService
                    .ConfirmImportRetro(formData)
                    .subscribe((successResult) => {
                      successMessage =
                        '<div class="retro-detail-import-message-result">' +
                        successResult.result.join("<br/>") +
                        "</div>";
                      abp.message.success(
                        successMessage,
                        `Inport thành công cho ${successResult.result.length} user:`,
                        true
                      );
                      this.close(true);
                    });
                }
              },
              true
            );
          } else if (
            this.results.listEmailSuccess != null &&
            this.results.listEmailSuccess.length > 0
          ) {
            message =
              '<div class="retro-detail-import-message-result">' +
              this.results.listEmailSuccess.join("<br/>") +
              "<div>";
            abp.message.success(
              message,
              `Import thành công cho ${this.results.listEmailSuccess.length} user:`,
              true
            );
            this.close(true);
          }
        }
      },
      (err) => {
        this.isSaving = false;
        abp.message.error(err);
      }
    );
  }
  public onProjectSelect(projectName: string) {
    this.selectedProjectName = projectName;
  }

  public getFailMessage(failedList) {
    let messages = "";
    failedList.forEach((mess, index) => {
      messages += `<div class='row'>
          <div class='col-md-1'>
            ${index + 1}
          </div>
          <div class='col-md-11 text-left' >${mess}</div>
        </div>`;
    });
    return messages;
  }
  public getWarningMessage(waringMessages) {
    let messages = "";

    waringMessages.forEach((mess, index) => {
      messages += `<div class='row'>
          <div class='col-md-1'>
            ${index + 1}
          </div>
          <div class='col-md-11 text-left' >${mess}</div>
        </div>`;
    });
    return messages;
  }
  public getSuccesMessage(succesMessages) {
    let messages = "";
    succesMessages.forEach((mess, index) => {
      messages += `<div class='row'>
          <div class='col-md-1'>
            ${index + 1}
          </div>
          <div class='col-md-11 text-left ' >${mess}</div>
        </div>`;
    });
    return messages;
  }
  close(res): void {
    this._dialogRef.close(res);
  }
}
