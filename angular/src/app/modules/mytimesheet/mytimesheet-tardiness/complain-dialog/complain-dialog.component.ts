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
  
  // Tracking initial state
  initialUserNotes: { [key: number]: string } = {};
  checkedPunishments: Set<number> = new Set();

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
        this.initialUserNotes[note.punishmentType] = note.userNote;
        this.checkedPunishments.add(note.punishmentType);
      });
    }
  }

  ngOnInit() {
  }

  isPunishmentSelected(typeValue: number): boolean {
    return this.checkedPunishments.has(typeValue);
  }

  shouldShowTextarea(typeValue: number): boolean {
    return this.checkedPunishments.has(typeValue);
  }

  onCheckboxChange(event: any, typeValue: number): void {
    if (event.checked) {
      this.checkedPunishments.add(typeValue);
      if (this.initialUserNotes[typeValue]) {
        this.userNotes[typeValue] = this.initialUserNotes[typeValue];
      } else {
        this.userNotes[typeValue] = '';
      }
    } else {
      this.checkedPunishments.delete(typeValue);
      this.userNotes[typeValue] = '';
    }
  }

  onNoClick(): void {
    this.dialogRef.close();
  }

  onSubmit(): void {
    const complaintsToAdd = [];
    const complaintsToDelete = [];
    this.punishmentTypes.forEach(type => {
      const punishmentType = type.value;
      const isChecked = this.checkedPunishments.has(punishmentType);
      const hadInitialComplaint = this.initialUserNotes.hasOwnProperty(punishmentType);
      const currentNote = (this.userNotes[punishmentType] || '').trim();
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
        (userPunishment.userPunishmentId || userPunishment.id || userPunishment.Id || userPunishment.UserPunishmentId) : 
        null;
  
  
      if (!userPunishmentId) {
        return; 
      }
      if (hadInitialComplaint && !isChecked) {
        complaintsToDelete.push(userPunishmentId);
      }
      else if (isChecked && currentNote) {
        complaintsToAdd.push({
          punishmentType: punishmentType,
          userNote: currentNote,
          userPunishmentId: userPunishmentId
        });
      }
      else if (isChecked && !currentNote && hadInitialComplaint) {
        complaintsToDelete.push(userPunishmentId);
      }
    });
  
    this.dialogRef.close({
      complaints: complaintsToAdd,
      complaintsToDelete: complaintsToDelete
    });
  }
}