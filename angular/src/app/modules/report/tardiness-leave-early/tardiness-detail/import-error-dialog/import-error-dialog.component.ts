import { Component, Inject, OnInit } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material';

interface ErrorItem {
  row: string;
  description: string;
}

@Component({
  selector: 'app-import-error-dialog',
  templateUrl: './import-error-dialog.component.html',
  styleUrls: ['./import-error-dialog.component.css']
})
export class ImportErrorDialogComponent implements OnInit {
  parsedErrors: ErrorItem[] = [];

  constructor(
    public dialogRef: MatDialogRef<ImportErrorDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: { errors: string[] }
  ) { }

  ngOnInit(): void {
    this.parseErrors();
  }

  parseErrors(): void {
    if (!this.data.errors || this.data.errors.length === 0) {
      this.parsedErrors = [{ row: '-', description: 'Unknown error occurred' }];
      return;
    }

    const combinedErrors = this.data.errors.join(' ');
    const rowMatches = combinedErrors.match(/Row\s+\d+/g);
    if (combinedErrors.includes('Row') && rowMatches && rowMatches.length > 1) {
      console.log('Processing combined error message');

      const rowMatchesWithColonOrDot = combinedErrors.match(/Row\s+\d+[:.]/g) || [];
      
      if (rowMatchesWithColonOrDot.length > 0) {
        const result: ErrorItem[] = [];

        let errorText = combinedErrors;

        const prefixPattern = /^.*?validation errors\.\s*Please fix.*?issues and try again:\s*/i;
        errorText = errorText.replace(prefixPattern, '');

        const rowErrors = errorText.split(/Row\s+\d+[:.]/)
          .filter(text => text.trim().length > 0);

        const rowNumbers = rowMatchesWithColonOrDot.map(match => {
          const num = match.match(/\d+/);
          return num ? num[0] : '-';
        });

        for (let i = 0; i < rowErrors.length; i++) {
          if (i < rowNumbers.length) {
            result.push({
              row: rowNumbers[i],
              description: rowErrors[i].trim()
            });
          }
        }
        
        if (result.length > 0) {
          this.parsedErrors = result;
          return;
        }
      }
    }
    
    this.parsedErrors = this.data.errors.map(error => {

      if (error.match(/^Row\s+\d+:/i)) {
        const rowMatch = error.match(/^Row\s+(\d+):/i);
        const row = rowMatch ? rowMatch[1] : '-';
        const description = error.replace(/^Row\s+\d+:\s*/i, '').trim();
        return { row, description };
      } 

      else if (error.match(/^Row\s+\d+\.\s+/i)) {
        const rowMatch = error.match(/^Row\s+(\d+)\./i);
        const row = rowMatch ? rowMatch[1] : '-';
        const description = error.replace(/^Row\s+\d+\.\s*/i, '').trim();
        return { row, description };
      }

      else if (error.includes('Row')) {
        const rowMatch = error.match(/Row\s+(\d+)/i);
        if (rowMatch) {
          const row = rowMatch[1];
          return { row, description: error.trim() };
        }
      }

      return { row: '-', description: error.trim() };
    });
  }

  onClose(): void {
    this.dialogRef.close();
  }
}
