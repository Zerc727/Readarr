import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { clearPendingChanges } from 'Store/Actions/baseActions';
import { fetchMetadataSource, saveMetadataSource, setMetadataSourceValue } from 'Store/Actions/settingsActions';
import createSettingsSectionSelector from 'Store/Selectors/createSettingsSectionSelector';
import MetadataSource from './MetadataSource';

const SECTION = 'metadataSource';

function createMapStateToProps() {
  return createSelector(
    (state: { settings: { advancedSettings: boolean } }) => state.settings.advancedSettings,
    createSettingsSectionSelector(SECTION),
    (advancedSettings, sectionSettings) => {
      return {
        advancedSettings,
        ...sectionSettings
      };
    }
  );
}

const mapDispatchToProps = {
  dispatchFetchMetadataSource: fetchMetadataSource,
  dispatchSetMetadataSourceValue: setMetadataSourceValue,
  dispatchSaveMetadataSource: saveMetadataSource,
  dispatchClearPendingChanges: clearPendingChanges
};

interface MetadataSourceConnectorProps {
  isSaving: boolean;
  hasPendingChanges: boolean;
  dispatchFetchMetadataSource: () => void;
  dispatchSetMetadataSourceValue: (payload: { name: string; value: unknown }) => void;
  dispatchSaveMetadataSource: () => void;
  dispatchClearPendingChanges: (payload: { section: string }) => void;
  onChildMounted: (saveCallback: () => void) => void;
  onChildStateChange: (payload: { isSaving: boolean; hasPendingChanges: boolean }) => void;
}

class MetadataSourceConnector extends Component<MetadataSourceConnectorProps> {

  //
  // Lifecycle

  componentDidMount() {
    const {
      dispatchFetchMetadataSource,
      dispatchSaveMetadataSource,
      onChildMounted
    } = this.props;

    dispatchFetchMetadataSource();
    onChildMounted(dispatchSaveMetadataSource);
  }

  componentDidUpdate(prevProps: MetadataSourceConnectorProps) {
    const {
      hasPendingChanges,
      isSaving,
      onChildStateChange
    } = this.props;

    if (
      prevProps.isSaving !== isSaving ||
      prevProps.hasPendingChanges !== hasPendingChanges
    ) {
      onChildStateChange({
        isSaving,
        hasPendingChanges
      });
    }
  }

  componentWillUnmount() {
    this.props.dispatchClearPendingChanges({ section: 'settings.metadataSource' });
  }

  //
  // Listeners

  onInputChange = ({ name, value }: { name: string; value: unknown }) => {
    this.props.dispatchSetMetadataSourceValue({ name, value });
  };

  //
  // Render

  render() {
    return (
      <MetadataSource
        onInputChange={this.onInputChange}
        {...this.props}
      />
    );
  }
}

export default connect(createMapStateToProps, mapDispatchToProps)(MetadataSourceConnector);
