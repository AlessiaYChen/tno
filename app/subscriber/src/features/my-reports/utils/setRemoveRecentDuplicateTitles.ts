import { IReportForm } from '../interfaces';

/**
 * Applies the supplied recent duplicate filtering preference to the report and all sections.
 * @param report Report form values.
 * @param checked Whether duplicate filtering should be enabled.
 * @returns Updated report form with duplicate filtering synchronized.
 */
export const setRemoveRecentDuplicateTitles = (report: IReportForm, checked: boolean) => ({
  ...report,
  removeRecentDuplicateTitles: checked,
  settings: {
    ...report.settings,
    sections: {
      ...report.settings.sections,
      removeRecentDuplicateTitles: checked,
    },
  },
  sections: report.sections.map((section) => ({
    ...section,
    settings: {
      ...section.settings,
      removeRecentDuplicateTitles: checked,
    },
  })),
});
