import { Component, OnInit, Inject } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material';
import { TimekeepingDto } from '../../../../service/api/model/report-timesheet-Dto';

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
    @Inject(MAT_DIALOG_DATA) public data: ComplainDialogData
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

  ngOnInit() {
  }

  onCheckboxChange(event: any, typeValue: number): void {
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
    const result = Object.keys(this.userNotes).map(key => {
      const punishmentType = parseInt(key);

      let userPunishment = this.userPunishments.find(up => {
        return up.type === punishmentType || 
               up.userPunishmentType === punishmentType || 
               up.Type === punishmentType;
      });

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
      
      return {
        punishmentType: punishmentType,
        userNote: this.userNotes[punishmentType],
        userPunishmentId: userPunishmentId
      };
    }).filter(item => item.userPunishmentId);  

    if (result.length > 0) {
      this.dialogRef.close(result);
    } else {
      alert('Không thể gửi khiếu nại. Vui lòng kiểm tra lại thông tin và đảm bảo bạn đã chọn loại phạt hợp lệ.');
    }
  }
}
