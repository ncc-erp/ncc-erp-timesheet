import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CheckBoardComponent } from './check-board.component';
import { CheckBoardRoutingModule } from './check-board-routing.module';
import { SharedModule } from '@shared/shared.module';
import { ReactiveFormsModule, FormsModule } from '@angular/forms';
import { NgxPaginationModule } from 'ngx-pagination';
import { CreateCheckBoardComponent } from './create-check-board/create-check-board.component';
import { NgxMatSelectSearchModule } from 'ngx-mat-select-search';

@NgModule({
  declarations: [CheckBoardComponent, CreateCheckBoardComponent],
  imports: [
    CommonModule,
    CheckBoardRoutingModule,
    SharedModule,
    ReactiveFormsModule,
    NgxPaginationModule,
    FormsModule,
    NgxMatSelectSearchModule
  ],
  entryComponents: [
    CreateCheckBoardComponent
  ],
  exports: [
    
  ]
})
export class CheckBoardModule { }
