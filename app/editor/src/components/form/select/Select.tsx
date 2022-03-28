import { Row } from 'components/flex/row';
import React, { Ref } from 'react';
import { ActionMeta, GroupBase, Props } from 'react-select';
import ReactSelect from 'react-select/dist/declarations/src/Select';

import { FieldSize, IOptionItem } from '..';
import { SelectVariant } from '.';
import * as styled from './styled';

export interface ISelectBaseProps {
  /**
   * Name of the form field.
   */
  name: string;
  /**
   * The label to include with the control.
   */
  label?: string;
  /**
   * The styled variant.
   */
  variant?: SelectVariant;
  /**
   * The tooltip to show on hover.
   */
  tooltip?: string;
  /**
   * Whether this form field is required.
   */
  required?: boolean;
  /**
   * Size of field.
   */
  width?: FieldSize;
  /**
   * Error message to display if validation fails.
   */
  error?: string;
}

export type SelectProps = ISelectBaseProps &
  Props &
  Omit<React.HTMLAttributes<HTMLSelectElement>, 'defaultValue' | 'onChange'>;

export interface ISelectProps<OptionType> extends SelectProps {
  ref?: Ref<ReactSelect<OptionType, boolean, GroupBase<OptionType>>>;
}

/**
 * Select component provides a bootstrapped styled button element.
 * @param param0 Select element attributes.
 * @returns Select component.
 */
export const Select = <OptionType extends IOptionItem>({
  id,
  ref,
  name,
  label,
  value,
  variant = SelectVariant.primary,
  tooltip,
  children,
  required,
  width = FieldSize.Stretch,
  error,
  classNamePrefix,
  className,
  options,
  onInput,
  onInvalid,
  onChange,
  ...rest
}: ISelectProps<OptionType>) => {
  const [errorMsg, setErrorMsg] = React.useState(error);
  const selectRef = React.useRef(null);
  const inputRef = React.useRef<HTMLInputElement>(null);

  return (
    <styled.Select className="frm-in">
      {label && <label htmlFor={`sel-${name}`}>{label}</label>}
      <Row>
        <styled.SelectField
          ref={selectRef}
          id={id ?? `sel-${name}`}
          name={name}
          className={`${className ?? ''}${!!errorMsg ? ' alert' : ''}`}
          classNamePrefix={classNamePrefix ?? 'rs'}
          data-for="main-tooltip"
          data-tip={tooltip}
          variant={variant}
          required={required}
          width={width}
          value={value}
          options={options}
          onChange={(newValue: unknown, actionMeta: ActionMeta<unknown>) => {
            if (onChange) onChange(newValue, actionMeta);
            inputRef?.current?.setCustomValidity('');
          }}
          onFocus={(e: any) => {
            const input = e.target as HTMLSelectElement;
            input?.setCustomValidity('');
            setErrorMsg(undefined);
          }}
          {...rest}
        />
        {children}
      </Row>
      {!rest.isDisabled && (
        <input
          ref={inputRef}
          tabIndex={-1}
          autoComplete="off"
          style={{
            opacity: 0,
            width: '100%',
            height: 0,
            position: 'absolute',
          }}
          value={(value as OptionType)?.value ?? ''}
          onChange={() => {}}
          onFocus={() => (selectRef.current as any)?.focus()}
          required={required}
          onInput={(e) => {
            const input = e.target as HTMLInputElement;
            input?.setCustomValidity('');
            setErrorMsg(undefined);
          }}
          onInvalid={(e) => {
            const input = e.target as HTMLInputElement;
            if (required && input?.validity.valueMissing) {
              input.setCustomValidity(error ?? 'required');
              setErrorMsg(error ?? 'required');
            }
          }}
        />
      )}
      {errorMsg && <p role="alert">{errorMsg}</p>}
    </styled.Select>
  );
};