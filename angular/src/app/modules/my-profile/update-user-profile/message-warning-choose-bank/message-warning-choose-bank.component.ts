import { Component, Injector, OnInit } from '@angular/core';
import { MatDialogRef } from '@angular/material';
import { AppComponentBase } from '@shared/app-component-base';

@Component({
  selector: 'app-message-warning-choose-bank',
  templateUrl: './message-warning-choose-bank.component.html',
  styleUrls: ['./message-warning-choose-bank.component.css']
})
export class MessageWarningChooseBankComponent extends AppComponentBase implements OnInit {

  constructor(injector: Injector,
    public dialogRef: MatDialogRef<MessageWarningChooseBankComponent>,) {
    super(injector);
  }

  ngOnInit() {
  }
  onSubmit(){
    this.dialogRef.close(true);
  }

}
