import { Component, Inject, OnInit } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material';

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
    @Inject(MAT_DIALOG_DATA) public data: { errors: string }
  ) { }

  ngOnInit(): void {
    this.parseErrors();
  }

  parseErrors(): void {
    if (!this.data.errors) {
      this.parsedErrors = [{ row: '-', description: 'Unknown error occurred' }];
      return;
    }

    const errorsArray = typeof this.data.errors === 'string' ? [this.data.errors] : this.data.errors;

    const validationPrefix = 'Import failed due to validation errors. Please fix the following issues and try again:';
    const cleanedErrors = Array.isArray(errorsArray) 
      ? errorsArray.map(err => typeof err === 'string' ? err.replace(validationPrefix, '').trim() : err)
      : errorsArray;

    const combinedErrors = Array.isArray(cleanedErrors) ? cleanedErrors.join(' ') : cleanedErrors;
    const rowMatches = combinedErrors.match(/Row\s+\d+/g);
    
    if (combinedErrors.includes('Row') && rowMatches && rowMatches.length > 1) {
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

    const errorLines = combinedErrors.split('\n')
      .filter(line => line.trim() !== '')
      .filter(line => !line.includes('Import failed due to validation errors'));
    
    this.parsedErrors = errorLines.map(error => {

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
