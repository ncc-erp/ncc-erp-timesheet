import { Component, OnInit, Inject } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material';
import { TimekeepingDto } from '../../../service/api/model/report-timesheet-Dto';
import { MatSnackBar } from '@angular/material/snack-bar';
export interface ComplainDialogData {
  timekeeping: TimekeepingDto;
  punishmentTypes: any[];
  structuredUserNotes: any[];
  userPunishments?: any[];
}

@Component({
  selector: 'app-complain-dialog',
  templateUrl: './complain-dialog.component.html',
  styleUrls: ['./complain-dialog.component.css']
})
export class ComplainDialogComponent implements OnInit {
  timekeeping: TimekeepingDto;
  punishmentTypes: any[] = [];
  userNotes: { [key: number]: string } = {};
  selectedPunishmentType: number;
  userPunishments: any[] = [];

  constructor(
    public dialogRef: MatDialogRef<ComplainDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: ComplainDialogData,
    private snackBar: MatSnackBar
  ) {
    this.timekeeping = data.timekeeping;
    this.punishmentTypes = data.punishmentTypes;
    this.userPunishments = data.userPunishments || [];
    if (data.structuredUserNotes && data.structuredUserNotes.length > 0) {
      data.structuredUserNotes.forEach(note => {
        this.userNotes[note.punishmentType] = note.userNote;
      });
    }
  }
  hasValidComplaint(): boolean {
    return Object.keys(this.userNotes).some(
      key => this.userNotes[key] && this.userNotes[key].trim() !== ''
    );
  }
  ngOnInit() {
    console.log('punishmentTypes:', this.punishmentTypes);
    console.log('userPunishments:', this.userPunishments);
    console.log('structuredUserNotes:', this.data.structuredUserNotes);
  }

  onCheckboxChange(event: any, typeValue: number): void {
    typeValue = Number(typeValue);
    if (event.checked) {
      this.userNotes[typeValue] = this.userNotes[typeValue] || '';
    } else {
      delete this.userNotes[typeValue];
    }
  }

  onNoClick(): void {
    this.dialogRef.close();
  }

  onSubmit(): void {
    console.log('Submit button clicked');
    console.log('User punishments:', this.userPunishments);
    console.log('User notes:', this.userNotes);

    if (this.userPunishments && this.userPunishments.length > 0) {
      console.log('First punishment structure:', JSON.stringify(this.userPunishments[0]));
      console.log('Available fields:', Object.keys(this.userPunishments[0]));
    }
    const result = Object.keys(this.userNotes).map(key => {
      const punishmentType = parseInt(key);
      let userPunishment = this.userPunishments.find(up => {
        console.log(`Checking punishment record:`, up);
        console.log(`Looking for type ${punishmentType}, record has:`, {
          type: up.type,
          userPunishmentType: up.userPunishmentType,
          Type: up.Type
        });
        return up.type === punishmentType ||
          up.userPunishmentType === punishmentType ||
          up.Type === punishmentType;
      });

      console.log(`Searching for punishment type ${punishmentType}:`, userPunishment);
      if (!userPunishment && this.data.userPunishments) {
        userPunishment = this.data.userPunishments.find(up => {
          return up.type === punishmentType ||
            up.userPunishmentType === punishmentType ||
            up.Type === punishmentType;
        });
      }
      const userPunishmentId = userPunishment ?
        (userPunishment.id || userPunishment.Id || userPunishment.userPunishmentId || userPunishment.UserPunishmentId) :
        null;

      console.log(`Found userPunishmentId: ${userPunishmentId} for type ${punishmentType}`);

      return {
        punishmentType: punishmentType,
        userNote: this.userNotes[punishmentType],
        userPunishmentId: userPunishmentId
      };
    }).filter(item => item.userNote && item.userNote.trim() !== '' && item.userPunishmentId);

    console.log('Result to be returned:', result);
    if (result.length > 0) {
      this.dialogRef.close(result);
    } else {
      this.snackBar.open(
        'Không thể gửi khiếu nại. Vui lòng kiểm tra lại thông tin và đảm bảo bạn đã chọn loại phạt hợp lệ.',
        'Đóng',
        { duration: 4000, panelClass: ['snackbar-error'] }
      );
    }
  }
}
