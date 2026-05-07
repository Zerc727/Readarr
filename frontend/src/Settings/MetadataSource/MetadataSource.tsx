import React from 'react';
import Alert from 'Components/Alert';
import FieldSet from 'Components/FieldSet';
import Form from 'Components/Form/Form';
import FormGroup from 'Components/Form/FormGroup';
import FormInputGroup from 'Components/Form/FormInputGroup';
import FormLabel from 'Components/Form/FormLabel';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import { inputTypes, kinds } from 'Helpers/Props';
import translate from 'Utilities/String/translate';

interface MetadataSourceProps {
  isFetching: boolean;
  error?: object;
  settings: Record<string, unknown>;
  hasSettings: boolean;
  onInputChange: (change: { name: string; value: unknown }) => void;
}

function MetadataSource(props: MetadataSourceProps) {
  const {
    isFetching,
    error,
    settings,
    hasSettings,
    onInputChange
  } = props;

  return (
    <div>
      {
        isFetching &&
          <LoadingIndicator />
      }

      {
        !isFetching && error &&
          <Alert kind={kinds.DANGER}>
            {translate('UnableToLoadMetadataSourceSettings')}
          </Alert>
      }

      {
        hasSettings && !isFetching && !error &&
          <Form>
            <FieldSet legend={translate('MetadataSource')}>
              <FormGroup>
                <FormLabel>
                  {translate('MetadataSource')}
                </FormLabel>

                <FormInputGroup
                  type={inputTypes.TEXT}
                  name="metadataSource"
                  helpText={translate('MetadataSourceHelpText')}
                  onChange={onInputChange}
                  {...settings.metadataSource as object}
                />
              </FormGroup>
            </FieldSet>
          </Form>
      }
    </div>
  );
}

export default MetadataSource;
