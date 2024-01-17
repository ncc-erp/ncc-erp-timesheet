import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { CreateInterviewNoteComponent } from './create-interview-note.component';

describe('CreateInterviewNoteComponent', () => {
  let component: CreateInterviewNoteComponent;
  let fixture: ComponentFixture<CreateInterviewNoteComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ CreateInterviewNoteComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(CreateInterviewNoteComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
