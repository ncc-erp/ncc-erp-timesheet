import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { CreatePmNoteComponent } from './create-pm-note.component';

describe('CreatePmNoteComponent', () => {
  let component: CreatePmNoteComponent;
  let fixture: ComponentFixture<CreatePmNoteComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ CreatePmNoteComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(CreatePmNoteComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
