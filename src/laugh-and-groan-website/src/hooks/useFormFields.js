// https://serverless-stack.com/chapters/create-a-custom-react-hook-to-handle-form-fields.html
import { useState } from "react";

const useFormFields = (initialState) => {
  const [fields, setValues] = useState(initialState);

  return [
    fields,
    function (event) {
      setValues({
        ...fields,
        [event.target.id]: event.target.value,
      });
    },
  ];
}

export default useFormFields;
