// ReSharper disable ClassNeverInstantiated.Global

namespace Linko.LinkoExchange.Services.Base
{
    public static class ErrorConstants
    {
        public class Validator
        {
            #region static fields and constants

            public const string PropertyNameIsRequired = "{PropertyName} is required.";

            #endregion
        }

        public class SampleImport
        {
            #region static fields and constants

            public const string CannotFindImportFile = "Cannot find import file.";
            public const string DataProviderDoesNotExist = "The Data Provider no longer exists.";
            public const string DefaultCollectionMethodIsRequired = "Default Collection Method is required.";
            public const string DefaultMonitoringPointIsRequired = "Default Monitoring Point is required.";
            public const string DefaultSampleTypeIsRequired = "Default Sample Type is required.";
            public const string DataTranslationsAreRequired = "Data Translation(s) are required.";
            public const string ImportTemplateDoesNotExist = "No file format has been defined. Contact your Authority for assistance.";
            public const string QueryParameterIsInvalid = "Query Parameter {0} is not valid.";
            public const string ImportSummaryIsOutDate = "The Import is done, please Import Another File.";
            public const string LinkoExchangeTermNoLongerAvailable = "{0} \"{1}\" is no longer available.";

            #endregion

            public class FileValidation
            {
                #region static fields and constants

                public const string FieldValueExceedMaximumSize = "The length of {0} exceeds the maximum of size of {1}.";
                public const string FieldValueIsNotBoolean = "{0} is not boolean.";
                public const string FieldValueIsNotDate = "{0} is not a valid date.";
                public const string FieldValueIsNotNumeric = "{0} is not numeric.";
                public const string FieldValueIsRequired = "{0} is required and is missing.";
                public const string FileTypeIsUnsupported = "The file type selected is not an .xlsx file.";

                public const string ImportFileExceedSizeLimitation = "The file size exceeds the maximum supported size of {0} MB.  Try splitting the data into 2 files.";
                public const string ImportFileIsCorrupted = "The file type selected in not an .xlsx file or it may be corrupted.";
                public const string ImportFileIsEmpty = "The file is empty.";
                public const string ImportFileMissingRequiredFields = "The file does not contain the required column(s) {0}.";
                public const string ResultIsRequired = "Result is required.";
                public const string ResultQualifierIsInvalid = "Result Qualifier of {0} is not valid.";
                public const string ResultQualifierNdNfShouldNotHaveAValue = "A Result Qualifier of ND or NF cannot be followed by a value in the Result field.";

                #endregion
            }

            public class DataValidation
            {
                #region static fields and constants

                public const string FieldValueIsRequired = "{0} is required and is missing.";

                public const string DuplicateParametersInSameSample = "Duplicate parameters exist for the same sample.";
                public const string FlowUnitIsInvalidOnMassLoadingCalculation = "Invalid Flow unit for Mass Loadings calculations. Chosen unit must be {0}.";
                public const string FlowUnitIsUnSpecified = "Missing flow units for Mass Loading calculations.";
                public const string FlowValueIsInvalid = "Missing flow value for Mass Loading calculations.";
                public const string ParameterUnitIsUnspecified = "Parameter Unit is unspecified, Contact your Authority for assistance.";
                public const string FlowResultQualifierMustBeEmpty = "Result Qualifier must be empty for the Mass Loading Flow parameter.";

                public const string TranslatedUnitDoesNotSupportUnitConversion =
                    "Invalid unit translation. Cannot convert {0} to {1}.  Check the unit translation selected or contact your Authority.";

                #endregion
            }
        }

        public class Unit
        {
            public const string PropertySystemUnitCannotBeNull = "Property SystemUnit should not be null";
            public const string UnsupportedUnitConversion = "Current unit and target unit are not in same unit dimension.";
        }
    }
}