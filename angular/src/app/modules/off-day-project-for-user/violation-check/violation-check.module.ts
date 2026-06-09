import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { SharedModule } from '@shared/shared.module';
import { DragDropModule } from '@angular/cdk/drag-drop';
import { ViolationCheckComponent } from './violation-check.component';

@NgModule({
  declarations: [ViolationCheckComponent],
  imports: [
    CommonModule,
    SharedModule,
    FormsModule,
    DragDropModule,
  ]
})
export class ViolationCheckModule { }
