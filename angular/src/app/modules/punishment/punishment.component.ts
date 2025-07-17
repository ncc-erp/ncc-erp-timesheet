import { Component, ElementRef, Injector, OnInit, ViewChild } from '@angular/core';
import { MatDialog } from '@angular/material';
import { finalize } from 'rxjs/operators';
import * as FileSaver from 'file-saver';

import { appModuleAnimation } from '@shared/animations/routerTransition';
import { PagedListingComponentBase, PagedRequestDto, PagedResultDto } from '@shared/paged-listing-component-base';
import { PERMISSIONS_CONSTANT } from '@app/constant/permission.constant';

import { PunishmentService, PunishmentDto, PunishmentType } from './../../service/api/punishment.service';
import { CreateEditPunishmentComponent } from './create-edit-punishment/create-edit-punishment.component';
import { ImportErrorDialogComponent } from './import-error-dialog/import-error-dialog.component';

interface PunishmentResponse {
  result: {
    items: PunishmentDto[];
    totalCount: number;
  };
  success: boolean;
  error: any;
  unAuthorizedRequest: boolean;
  __abp: boolean;
}

@Component({
  selector: 'app-punishment',
  templateUrl: './punishment.component.html',
  styleUrls: ['./punishment.component.css'],
  animations: [appModuleAnimation()]
})

export class PunishmentComponent extends PagedListingComponentBase<PunishmentDto> implements OnInit {
  ADD_PUNISHMENT = PERMISSIONS_CONSTANT.AddPunishments;
  EDIT_PUNISHMENT = PERMISSIONS_CONSTANT.EditPunishments;
  DELETE_PUNISHMENT = PERMISSIONS_CONSTANT.DeletePunishments;

  punishments: PunishmentDto[] = [];
  searchText: string = '';
  isTableLoading = false;
  isDownloading = false;
  fileToUpload: File = null;

  constructor(
    private punishmentService: PunishmentService,
    private _dialog: MatDialog,
    injector: Injector
  ) {
    super(injector);
  }

  ngOnInit() {
    this.refresh();
  }

  protected list(
    request: PagedRequestDto,
    pageNumber: number,
    finishedCallback: () => void
  ): void {
    this.isTableLoading = true;
    this.punishmentService
      .getAllPunishments()
      .pipe(finalize(() => {
        finishedCallback();
        this.isTableLoading = false;
      }))
      .subscribe((response: PunishmentResponse) => {
        if (response && response.result && response.result.items) {
          const items = response.result.items;
        }

        const result: PagedResultDto = {
          items: response.result.items,
          totalCount: response.result.totalCount
        };
        this.punishments = response.result.items;
        this.showPaging(result, pageNumber);
      });
  }

  searchOrFilter() {
    if (this.searchText) {
      const searchLower = this.searchText.toLowerCase();
      this.punishments = this.punishments.filter(punishment =>
        punishment.name.toLowerCase().includes(searchLower) ||
        (punishment.description && punishment.description.toLowerCase().includes(searchLower))
      );
    } else {
      this.refresh();
    }
  }

  refresh() {
    this.searchText = '';
    this.getDataPage(1);
  }

  createPunishment() {
    const punishment = {} as PunishmentDto;
    this.showDialog(punishment);
  }

  editPunishment(punishment: PunishmentDto): void {
    this.showDialog(punishment);
  }

  private showDialog(punishment: PunishmentDto): void {
    const dialogRef = this._dialog.open(CreateEditPunishmentComponent, {
      data: punishment,
      disableClose: true,
      panelClass: 'punishment-dialog-container'
    });

    dialogRef.afterClosed().subscribe((res) => {
      if (res) {
        if (punishment.id == null) {
          this.getDataPage(1);
        } else {
          this.getDataPage(this.pageNumber);
        }
      }
    });
  }

  protected delete(punishment: PunishmentDto): void {
    abp.message.confirm(
      `Delete punishment '${punishment.name}'?`,
      'Are you sure?',
      (result: boolean) => {
        if (result) {
          this.punishmentService.delete(punishment.id).subscribe(() => {
            abp.notify.success('Successfully deleted!');
            this.refresh();
          });
        }
      }
    );
  }

  deactivatePunishment(punishment: PunishmentDto): void {
    abp.message.confirm(
      `Deactivate punishment '${punishment.name}'?`,
      'Are you sure?',
      (result: boolean) => {
        if (result) {
          this.punishmentService.deactivate(punishment).subscribe(() => {
            abp.notify.success('Punishment deactivated successfully!');
            this.refresh();
          });
        }
      }
    );
  }

  activatePunishment(punishment: PunishmentDto): void {
    abp.message.confirm(
      `Activate punishment '${punishment.name}'?`,
      'Are you sure?',
      (result: boolean) => {
        if (result) {
          this.punishmentService.activate(punishment).subscribe(() => {
            abp.notify.success('Punishment activated successfully!');
            this.refresh();
          });
        }
      }
    );
  }

  downloadTemplate(): void {
    this.isDownloading = true;
    this.punishmentService.downloadTemplateImportPunishment()
      .pipe(finalize(() => this.isDownloading = false))
      .subscribe((response) => {
        if (response && response.result && response.result.base64) {
          try {
            const byteCharacters = atob(response.result.base64);
            const byteNumbers = new Array(byteCharacters.length);
            for (let i = 0; i < byteCharacters.length; i++) {
              byteNumbers[i] = byteCharacters.charCodeAt(i);
            }
            const byteArray = new Uint8Array(byteNumbers);

            const file = new Blob([byteArray], {
              type: response.result.fileType || "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            });
            FileSaver.saveAs(file, response.result.fileName || "TemplateImportPunishmentSystem.xlsx");
          } catch (e) {
            this.notify.error(this.l('DownloadTemplateFailed'));
          }
        } else {
          this.notify.error(this.l('DownloadTemplateFailed'));
        }
      }, error => {
        this.notify.error(this.l('DownloadTemplateFailed'));
      });
  }

  handleFileInput(files: FileList): void {
    this.fileToUpload = files.item(0);
  }

  @ViewChild('fileInput') fileInput: ElementRef;

  private originalMessageError: (message: string, title?: string, isHtml?: boolean) => any;
  private originalNotifyError: (message: string, title?: string, options?: any) => void;

  importFromFile(): void {
    if (!this.fileToUpload) {
      this.notify.error(this.l('Please select a file'));
      return;
    }

    const formData = new FormData();
    formData.append('File', this.fileToUpload, this.fileToUpload.name);

    this.originalMessageError = abp.message.error;
    this.originalNotifyError = abp.notify.error;

    abp.message.error = () => { };
    abp.notify.error = () => { };

    const toastContainer = document.querySelector('.toast-container');
    if (toastContainer) {
      const errorToasts = toastContainer.querySelectorAll('.toast-error');
      errorToasts.forEach(toast => toast.remove());
    }

    this.isTableLoading = true;
    this.punishmentService.importPunishmentFromFile(formData)
      .pipe(finalize(() => {
        this.isTableLoading = false;
        this.fileToUpload = null;
        if (this.fileInput && this.fileInput.nativeElement) {
          this.fileInput.nativeElement.value = '';
        }
      }))
      .subscribe(response => {
        abp.message.error = this.originalMessageError;
        abp.notify.error = this.originalNotifyError;

        if (response) {
          this.notify.success(
            this.l('Import Successful', response.successCount),
            this.l('Import Result', response.successCount, response.failedCount, response.errorCount)
          );
          if (response.failedCount > 0 || response.errorCount > 0) {
          }
          this.refresh();
        }
      }, (error) => {
        const toastContainer = document.querySelector('.toast-container');
        if (toastContainer) {
          const errorToasts = toastContainer.querySelectorAll('.toast-error');
          errorToasts.forEach(toast => toast.remove());
        }

        let errorMessages: string[] = [];
        let rawErrorMessage = '';

        if (error && error.error) {
          if (error.error.error && error.error.error.message) {
            rawErrorMessage = error.error.error.message;
          } else if (error.error.message) {
            rawErrorMessage = error.error.message;
          } else if (typeof error.error === 'string') {
            rawErrorMessage = error.error;
          }

          if (rawErrorMessage.includes('Import failed due to validation errors') ||
            rawErrorMessage.includes('Row') ||
            rawErrorMessage.includes('Invalid date format')) {
            errorMessages = this.parseImportErrors(rawErrorMessage);
          } else {
            errorMessages = [rawErrorMessage || 'Import failed. Please try again.'];
          }
        } else if (error && error.message) {
          errorMessages = [error.message];
        } else {
          errorMessages = ['Import failed. Please try again.'];
        }

        const dialogRef = this._dialog.open(ImportErrorDialogComponent, {
          width: '600px',
          data: { errors: errorMessages },
          disableClose: false
        });

        dialogRef.afterClosed().subscribe(() => {
          abp.message.error = this.originalMessageError;
          abp.notify.error = this.originalNotifyError;
        });
      });
  }

  private parseImportErrors(errorMessage: string): string[] {
    if (!errorMessage) return ['Unknown error occurred'];

    const validationPrefix = 'Import failed due to validation errors. Please fix the following issues and try again:';
    let cleanMessage = errorMessage.replace(validationPrefix, '').trim();

    if (cleanMessage.includes('\n')) {
      return cleanMessage.split('\n')
        .filter(line => line.trim() !== '')
        .map(line => line.trim());
    }

    const rowMatches = cleanMessage.match(/Row\s+\d+[:.]/g);
    if (rowMatches && rowMatches.length > 0) {
      const parts = cleanMessage.split(/Row\s+\d+[:.]/)
        .filter(part => part.trim().length > 0);

      const rowNumbers = rowMatches.map(match => {
        const num = match.match(/\d+/);
        return num ? num[0] : '';
      });

      const result = [];
      for (let i = 0; i < Math.min(parts.length, rowNumbers.length); i++) {
        result.push(`Row ${rowNumbers[i]}: ${parts[i].trim()}`);
      }

      return result.length > 0 ? result : [cleanMessage];
    }
    return [cleanMessage];
  }
}
